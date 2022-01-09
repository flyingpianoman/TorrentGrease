using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestUtils.Terminal
{
    public sealed class LocalTerminal : IAsyncDisposable
    {
        private readonly SemaphoreSlim _executeLock;
        private readonly Process _process;
        private readonly StreamWriter _inWriter;
        private readonly List<string> _outputLines;
        private readonly List<string> _errorLines;
        private ErrorCode? _errorCode;

        private TaskCompletionSource _readyForNextCommandTcs;
        private bool _receivedSignal;
        private string _readyForNextCommandSignalString;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        //Fields are set via a private method
        public LocalTerminal()
        {
            _executeLock = new SemaphoreSlim(1);
            _outputLines = new List<string>();
            _errorLines = new List<string>();

            _process = new Process();

            SetCorrectTerminalForPlatform();
            ConfigureProcess();

            Reset();

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            _inWriter = _process.StandardInput;

            SendAndWaitForCommandCompletionSignal(TimeSpan.FromSeconds(5));
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        private void Reset()
        {
            _errorCode = null;
            _outputLines.Clear();
            _errorLines.Clear();
            _receivedSignal = false;
            _readyForNextCommandSignalString = Guid.NewGuid().ToString();
            _readyForNextCommandTcs = new TaskCompletionSource();
        }

        private void SetCorrectTerminalForPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Might cause issues for platforms that don't have sh, we'll fix it when an issue is raised
                _process.StartInfo.FileName = "sh";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _process.StartInfo.FileName = "cmd";
            }
            else
            {
                throw new NotImplementedException("Only linux and windows are implemented atm");
            }
        }

        private void ConfigureProcess()
        {
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.EnableRaisingEvents = true;
            //hosted by the application itself to not open a black cmd window
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.CreateNoWindow = true;

            _process.OutputDataReceived += OnOutputDataReceived;
            _process.ErrorDataReceived += OnErrorDataReceived;
            _process.Exited += OnExited;
        }

        public Task<string> ExecuteAsync(string command)
        {
            return ExecuteAsync(command, TimeSpan.FromSeconds(30), executionDir: null);
        }

        public async Task<string> ExecuteAsync(string command, TimeSpan timeout, string? executionDir)
        {
            await _executeLock.WaitAsync().ConfigureAwait(false);
            try
            {
                Reset();
                await _inWriter.WriteLineAsync(command).ConfigureAwait(false);

                SendAndWaitForCommandCompletionSignal(timeout);

                CheckForError();

                return ParseOutput();
            }
            finally
            {
                _executeLock.Release();
            }
        }

        private void CheckForError()
        {
            if (_errorCode != null)
            {
                var errorMsg = _errorCode.Value switch
                {
                    ErrorCode.CommandError => "An error occurred while executing the command",
                    ErrorCode.CommandTimeout => "Command did not complete before the timeout expired",
                    ErrorCode.UnexpectedTerminalExit => "An unknown error occurred",
                    ErrorCode.Unknown => "An unknown error occurred",
                    _ => "An unknown error occurred",
                };

                throw new TerminalCommandException(errorMsg,
                    _errorCode.Value, ParseOutput(), String.Join(Environment.NewLine, _errorLines));
            }
        }

        private string ParseOutput()
        {
            //Filter out: 
            // - the initial command
            // - the empty line after the initial command
            // - the signalstring echo command
            return String.Join(Environment.NewLine, _outputLines
                .Skip(1)
                .Take(_outputLines.Count - 3));
        }

        private void SendAndWaitForCommandCompletionSignal(TimeSpan timeout)
        {
            _inWriter.WriteLine($"echo {_readyForNextCommandSignalString}");
            if(!_readyForNextCommandTcs.Task.Wait(timeout))
            {
                _errorCode = ErrorCode.CommandTimeout;
            }
        }

        // Handle the dataevent
        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == _readyForNextCommandSignalString)
            {
                //We want to wait for the last empty line so that we won't pollute the next command output
                _receivedSignal = true;
            }
            else if (_receivedSignal && e.Data == string.Empty)
            {
                _readyForNextCommandTcs.TrySetResult();
            }
            else
            {
                _outputLines.Add(e.Data ?? string.Empty);
            }
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            _errorLines.Add(e.Data ?? string.Empty);
            _errorCode = ErrorCode.CommandError;
        }

        private void OnExited(object? sender, EventArgs e)
        {
            _errorLines.Add("Unexpected terminal exit");
            _errorCode = ErrorCode.UnexpectedTerminalExit;
        }

        public async ValueTask DisposeAsync()
        {
            _process.Kill(entireProcessTree: true);
            _process.Dispose();
            await _inWriter.DisposeAsync().ConfigureAwait(false);
        }
    }
}
