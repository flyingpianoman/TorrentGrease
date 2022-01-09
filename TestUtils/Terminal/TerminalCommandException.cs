using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TestUtils.Terminal
{
    [Serializable()]
    public class TerminalCommandException : Exception
    {
        public string Output { get; init; }
        public string ErrorMessage { get; init; }
        public ErrorCode ErrorCode { get; init; }

        public TerminalCommandException(string message, ErrorCode errorCode, string output, string errorMessage) 
            : base(message)
        {
            this.ErrorCode = errorCode;
            this.Output = output;
            this.ErrorMessage = errorMessage;
        }

        protected TerminalCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            var errorCodeStr = info.GetString(nameof(ErrorCode)) ?? throw new ArgumentException($"No {nameof(ErrorCode)} value supplied");
            this.ErrorCode = (ErrorCode) Enum.Parse(typeof(ErrorCode), errorCodeStr);

            this.Output = info.GetString(nameof(Output)) ?? throw new ArgumentException($"No {nameof(Output)} value supplied");
            this.ErrorMessage = info.GetString(nameof(ErrorMessage)) ?? throw new ArgumentException($"No {nameof(ErrorMessage)} value supplied");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }
            info.AddValue(nameof(ErrorCode), Enum.GetName(this.ErrorCode));
            info.AddValue(nameof(Output), this.Output);
            info.AddValue(nameof(ErrorMessage), this.ErrorMessage);
            base.GetObjectData(info, context);
        }
    }
}
