using System;
using System.Runtime.InteropServices;
using System.Text;

namespace USPS4_CSharp
{
    class USPS4
    {
        // UserGuide PDF @ "Return Code Definitions"
        public enum Code : int
        {
            USPS_FSB_ENCODER_API_SUCCESS = 0,
            USPS_FSB_ENCODER_API_SELFTEST_FAILED = 1,
            USPS_FSB_ENCODER_API_BAR_STRING_IS_NULL = 2,
            USPS_FSB_ENCODER_API_BYTE_CONVERSION_FAILED = 3,
            USPS_FSB_ENCODER_API_RETRIEVE_TABLE_FAILED = 4,
            USPS_FSB_ENCODER_API_CODEWORD_CONVERSION_FAILED = 5,
            USPS_FSB_ENCODER_API_CHARACTER_RANGE_ERROR = 6,
            USPS_FSB_ENCODER_API_TRACK_STRING_IS_NULL = 7,
            USPS_FSB_ENCODER_API_ROUTE_STRING_IS_NULL = 8,
            USPS_FSB_ENCODER_API_TRACK_STRING_BAD_LENGTH = 9,
            USPS_FSB_ENCODER_API_TRACK_STRING_HAS_INVALID_DATA = 10,
            USPS_FSB_ENCODER_API_TRACK_STRING_HAS_INVALID_DIGIT2 = 11,
            USPS_FSB_ENCODER_API_ROUTE_STRING_BAD_LENGTH = 12,
            USPS_FSB_ENCODER_API_ROUTE_STRING_HAS_INVALID_DATA = 13
        }

        // UserGuide PDF @ "Using the Encoding Software from C"
        [DllImport("usps4cb64.dll", CharSet = CharSet.Ansi, EntryPoint = "USPS4CB")]
        private static extern int USPS4CB_64(
            [MarshalAs(UnmanagedType.LPStr)] [In] string TrackPtr,      // Buffer of 20 chars + null char
            [MarshalAs(UnmanagedType.LPStr)] [In] string RoutePtr,      // Buffer of 11 chars + null char
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder BarPtr);     // Buffer of 65 chars + null char

        [DllImport("usps4cb32.dll", CharSet = CharSet.Ansi, EntryPoint = "USPS4CB")]
        private static extern int USPS4CB_32(
            [MarshalAs(UnmanagedType.LPStr)] [In] string TrackPtr,
            [MarshalAs(UnmanagedType.LPStr)] [In] string RoutePtr,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder BarPtr);

        public static string EncodeString(string Track, string Route)
        {
            var result = Encode(Track, Route, out string output);

            if (result != Code.USPS_FSB_ENCODER_API_SUCCESS)
                throw new Exception(string.Format("Failed to encode Track/Route. Error: {0}", ErrorToString(result)));

            return output;
        }

        public static Code Encode(string Track, string Route, out string Output)
        {
            StringBuilder sb = new StringBuilder(65);
            Code result;

            if (Environment.Is64BitProcess)
                result = (Code)USPS4CB_64(Track, Route, sb);
            else
                result = (Code)USPS4CB_32(Track, Route, sb);

            Output = sb.ToString();
            return result;
        }

        public static string ErrorToString(Code Result)
        {
            switch (Result)
            {
                case Code.USPS_FSB_ENCODER_API_SUCCESS: return "Successful completion";
                case Code.USPS_FSB_ENCODER_API_SELFTEST_FAILED: return "Internal self-test failed";
                case Code.USPS_FSB_ENCODER_API_BAR_STRING_IS_NULL: return "Output barcode string is null";
                case Code.USPS_FSB_ENCODER_API_BYTE_CONVERSION_FAILED: return "Encoder byte conversion failed";
                case Code.USPS_FSB_ENCODER_API_RETRIEVE_TABLE_FAILED: return "Encoder retrieve table failed";
                case Code.USPS_FSB_ENCODER_API_CODEWORD_CONVERSION_FAILED: return "Encoder codeword conversion failed";
                case Code.USPS_FSB_ENCODER_API_CHARACTER_RANGE_ERROR: return "Encoder character range error";
                case Code.USPS_FSB_ENCODER_API_TRACK_STRING_IS_NULL: return "Input Tracking Code is null";
                case Code.USPS_FSB_ENCODER_API_ROUTE_STRING_IS_NULL: return "Input Routing Code is null";
                case Code.USPS_FSB_ENCODER_API_TRACK_STRING_BAD_LENGTH: return "Input Tracking Code must have 20 digits";
                case Code.USPS_FSB_ENCODER_API_TRACK_STRING_HAS_INVALID_DATA: return "Input Tracking Code must contain digits 0–9";
                case Code.USPS_FSB_ENCODER_API_TRACK_STRING_HAS_INVALID_DIGIT2: return "Input Tracking Code second digit must contain digits 0–4";
                case Code.USPS_FSB_ENCODER_API_ROUTE_STRING_BAD_LENGTH: return "Input Routing Code must be 0, 5, 9, or 11 digits in length";
                case Code.USPS_FSB_ENCODER_API_ROUTE_STRING_HAS_INVALID_DATA: return "Input Routing Code must contain digits 0–9";
            }

            return "Unknown error";
        }
    }
}
