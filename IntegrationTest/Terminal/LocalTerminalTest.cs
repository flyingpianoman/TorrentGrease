using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestUtils.Terminal;

namespace IntegrationTest.Terminal
{
    [TestClass]
    public class LocalTerminalTest
    {
        [TestMethod]
        public async Task ExecuteAsync_Echo_EchoIsInOutput()
        {
            await using var sut = new LocalTerminal();
            var stringToEcho = "some string: L";

            var output = await sut.ExecuteAsync($"echo {stringToEcho}");

            output.Should().BeEquivalentTo(stringToEcho);
        }

        [TestMethod]
        public async Task ExecuteAsync_MultilineOutput_MultipleOutputLines()
        {
            await using var sut = new LocalTerminal();
            var stringToEcho1 = "some string: L";
            var stringToEcho2 = "some string: L2";

            var output = await sut.ExecuteAsync($"echo {stringToEcho1}& echo {stringToEcho2}");

            output.Should().BeEquivalentTo(stringToEcho1 + Environment.NewLine + stringToEcho2);
        }

        [TestMethod]
        public async Task ExecuteAsync_TwoEchCommands_EchosAreInOutput()
        {
            await using var sut = new LocalTerminal();
            var stringToEcho1 = "some string: L";
            var stringToEcho2 = "some string: L2";

            var output1 = await sut.ExecuteAsync($"echo {stringToEcho1}");
            var output2 = await sut.ExecuteAsync($"echo {stringToEcho2}");

            output1.Should().BeEquivalentTo(stringToEcho1);
            output2.Should().BeEquivalentTo(stringToEcho2);
        }

        [TestMethod]
        public async Task ExecuteAsync_SlowEcho_EchoIsInOutput()
        {
            await using var sut = new LocalTerminal();
            var stringToEcho = "some string: L";

            var output = await sut.ExecuteAsync($"powershell Start-Sleep -Milliseconds 250 & echo {stringToEcho}");

            output.Should().BeEquivalentTo(stringToEcho);
        }

        [TestMethod]
        public async Task ExecuteAsync_Timeout_TimeoutException()
        {
            await using var sut = new LocalTerminal();

            try
            {
                await sut.ExecuteAsync($"powershell Start-Sleep -Milliseconds 500", TimeSpan.FromMilliseconds(10), null);
                Assert.Fail();
            }
            catch (TerminalCommandException tce)
            {
                tce.ErrorCode.Should().Be(ErrorCode.CommandTimeout);
            }
        }

        [TestMethod]
        public async Task ExecuteAsync_CommandError_CommandException()
        {
            await using var sut = new LocalTerminal();

            try
            {
                await sut.ExecuteAsync($"type non-existing-file", TimeSpan.FromMilliseconds(10), null);
                Assert.Fail();
            }
            catch (TerminalCommandException tce)
            {
                tce.ErrorCode.Should().Be(ErrorCode.CommandError);
            }
        }

        [TestMethod]
        public async Task ExecuteAsync_ConcurrentCalls_SynchronizedAccess()
        {
            await using var sut = new LocalTerminal();
            
            //Create a tmp file 
            var tmpFileToLock = Path.GetTempFileName();
            await File.WriteAllTextAsync(tmpFileToLock, "a");

            //Lock the tmp file so we can block 2 terminal calls
            using var file = File.OpenWrite(tmpFileToLock);

            //Start concurrent terminal calls that try to lock the tmp file
            var are1 = new AutoResetEvent(false);
            var are2 = new AutoResetEvent(false);
            var task1 = Task.Run(() =>
            {
                are1.Set();
                return sut.ExecuteAsync($"powershell [System.IO.File]::Open('{tmpFileToLock}', 'Open', 'Write', 'None') & echo 1").GetAwaiter().GetResult();
            });
            var task2 = Task.Run(() =>
            {
                are1.Set();
                return sut.ExecuteAsync($"powershell [System.IO.File]::Open('{tmpFileToLock}', 'Open', 'Write', 'None') & echo 2").GetAwaiter().GetResult();
            });

            are1.WaitOne(TimeSpan.FromSeconds(3));
            are2.WaitOne(TimeSpan.FromSeconds(3));

            //Tasks should still be running, since the file is still locked
            task1.Status.Should().Be(TaskStatus.Running);
            task2.Status.Should().Be(TaskStatus.Running);

            //Unlock the file and wait for both tasks to be completed
            file.Close();
            Task.WhenAll(task1, task2).Wait();

            //Assert
            task1.Result.Should().Be("1");
            task2.Result.Should().Be("2");
        }
    }
}
