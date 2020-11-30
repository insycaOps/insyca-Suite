using inSyca.foundation.framework;
using inSyca.foundation.integration.biztalk.functions.diagnostics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace inSyca.foundation.integration.biztalk.functions
{
    /// <summary>
    /// 
    /// </summary>
    public class scripting
    {
        public static string ExtractFromDispatchMessage(string strSourceMessage)
        {
            Log.DebugFormat("ExtractFromDispatchMessage(string strSourceMessage {0}", strSourceMessage);

            try
            {
                XElement xmlSourceMessage = XElement.Parse(strSourceMessage);
                string xmlDecodedMessage = HttpUtility.HtmlDecode(xmlSourceMessage.Element("message").Descendants().FirstOrDefault().Value.Trim());
                return xmlDecodedMessage;

            }
            catch (Exception ex)
            {
                Log.Error($"ExtractFromDispatchMessage(string strSourceMessage {strSourceMessage}", ex);

                throw;
            }
        }

        static public object GetIdentityValue(object id, object label, object number)
        {
            Log.DebugFormat("GetIdentityValue(object id {0}, object label {1}, object number {2})", id, label, number);

            if (!String.IsNullOrEmpty(id.ToString()))
                return id;
            else if (!String.IsNullOrEmpty(number.ToString()))
                return number;
            else if (!String.IsNullOrEmpty(label.ToString()))
                return label;
            else
                return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="If"></param>
        /// <param name="Then"></param>
        /// <param name="Else"></param>
        /// <returns></returns>
        static public object IfThenElse(object If, object Then, object Else)
        {
            Log.DebugFormat("IfThenElse(object If {0}, object Then {1}, object Else {2})", If, Then, Else);

            bool bIf;

            try
            {
                bIf = Convert.ToBoolean(If);
            }
            catch (Exception ex)
            {
                Log.Warn(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { If, Then, Else }, "IfThenElse - convert boolean\n\rFirst Level Exception: {0}", new object[] { ex.Message }));

                try
                {
                    if (Convert.ToDouble(If) > 0)
                        bIf = true;
                    else
                        bIf = false;
                }
                catch (Exception exc)
                {
                    Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { If, Then, Else }, ex));

                    throw exc;
                }
            }

            if (bIf)
                return Then;
            else
                return Else;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="formatString">{STR}{NBR}{CON}{CTY}{ZIP}</param>
        /// <param name="returnString"></param>
        /// <param name="inputAddress"></param>
        /// <returns></returns>
        static public string ExtractAddressPart(string formatString, string returnString, string inputAddress)
        {
            inputAddress = inputAddress.Trim();

            const string streetNumberPattern = @"(?<streetNumber>\d+\w+\s*(-\s*\d+\w*\s)?)";
            const string locationPattern = @"(?<location>\w.*)";
            const string countryPattern = @"(?<country>\s*\w{2}\s*)";
            const string zipPattern = @"(?<zip>\s*\d{5}\s*)";
            const string cityPattern = @"(?<city>\s*\w.*\s*)";

            const string streetNumberKey = "streetNumber";
            const string countryKey = "country";
            const string cityKey = "city";
            const string zipKey = "zip";
            const string locationKey = "location";

            Match match = null;
            while (match == null)
            {
                // search for pattern 1: country zipcode city (e.g. '1St Floor, Stanmore House')
                match = Regex.Match(inputAddress, countryPattern + zipPattern + cityPattern, RegexOptions.IgnoreCase);

                if (match.Success && match.Groups.Count == 4)
                    break;

                // search for pattern 1: country zipcode city (e.g. '1St Floor, Stanmore House')
                match = Regex.Match(inputAddress, zipPattern + cityPattern, RegexOptions.IgnoreCase);

                if (match.Success && match.Groups.Count == 3)
                    break;

                //// search for pattern 2: location , floor number (e.g.: 'Dome Building, 2Nd Floor')
                //match = Regex.Match(inputAddress, locationPattern + commaPattern + floorPattern, RegexOptions.IgnoreCase);

                //if (match.Success && match.Groups.Count == 3)
                //    break;

                // search for pattern 3: street number + street name (e.g.: 'South Clifton Street 56A' or 'Nicholas Street 16B-18B')
                match = Regex.Match(inputAddress, locationPattern + streetNumberPattern, RegexOptions.IgnoreCase);

                if (match.Success && match.Groups.Count == 4)
                    break;

                break;
            }

            string returnValue = string.Empty;

            switch (returnString)
            {
                case "{STR}":
                    try
                    {
                        returnValue = match.Groups[locationKey].Success ? match.Groups[locationKey].Value : inputAddress;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { formatString, returnString, inputAddress }, ex));
                    }
                    break;
                case "{NBR}":
                    try
                    {
                        returnValue = match.Groups[streetNumberKey].Success ? match.Groups[streetNumberKey].Value : inputAddress;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { formatString, returnString, inputAddress }, ex));
                    }
                    break;
                case "{CON}":
                    try
                    {
                        returnValue = match.Groups[countryKey].Success ? match.Groups[countryKey].Value : inputAddress;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { formatString, returnString, inputAddress }, ex));
                    }
                    break;
                case "{CTY}":
                    try
                    {
                        returnValue = match.Groups[cityKey].Success ? match.Groups[cityKey].Value : inputAddress;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { formatString, returnString, inputAddress }, ex));
                    }
                    break;
                case "{ZIP}":
                    try
                    {
                        returnValue = match.Groups[zipKey].Success ? match.Groups[zipKey].Value : inputAddress;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { formatString, returnString, inputAddress }, ex));
                    }
                    break;
                default:
                    break;
            }

            Log.DebugFormat("ExtractAddressPart(string formatString {0}, string returnString {1}, string inputAddress {2})\nReturn Value: {3}", formatString, returnString, inputAddress, returnValue);

            return returnValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="separatorString"></param>
        /// <param name="partIndex"></param>
        /// <param name="inputString"></param>
        /// <returns></returns>
        static public string SplitString(string separatorString, int partIndex, string inputString)
        {
            string[] parts = inputString.Split(new string[] { separatorString }, int.MaxValue, StringSplitOptions.RemoveEmptyEntries);

            return parts[partIndex];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        static public string CapitalizeWords(string inputString)
        {
            Log.DebugFormat("CapitalizeWords(string inputString {0})", inputString);

            char[] array = inputString.ToCharArray();
            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
                if (array[i - 1] == ' ')
                    if (char.IsLower(array[i]))
                        array[i] = char.ToUpper(array[i]);

            return new string(array);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        static public string ToUpper(string inputString)
        {
            Log.DebugFormat("ToUpper(string inputString {0})", inputString);

            return inputString.ToUpper();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        static public string ToLower(string inputString)
        {
            Log.DebugFormat("ToLower(string inputString {0})", inputString);

            return inputString.ToLower();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputDateString"></param>
        /// <param name="defaultDateString"></param>
        /// <param name="inputDateFormat"></param>
        /// <param name="outputDateFormat"></param>
        /// <param name="inputCultureInfoName"></param>
        /// <param name="outputCultureInfoName"></param>
        /// <returns></returns>
        static public string FormatDate(string inputDateString, string defaultDateString, string inputDateFormat, string outputDateFormat, string inputCultureInfoName, string outputCultureInfoName)
        {
            Log.DebugFormat("FormatDate(string inputDateString {0}, string defaultDateString {1}, string inputDateFormat {2}, string outputDateFormat {3}, string inputCultureInfoName {4}, string outputCultureInfoName {5})", inputDateString, defaultDateString, inputDateFormat, outputDateFormat, inputCultureInfoName, outputCultureInfoName);

            string[] inputDateFormatArray = null;

            if (string.IsNullOrEmpty(inputDateFormat))
                inputDateFormatArray = new string[] { "dd.MM.yyyy", "yyyy-MM-dd", "dd/mmyyyy" };
            else
                inputDateFormatArray = new string[] { inputDateFormat };

            DateTime outputDateTime;
            string outputDateTimeString = string.Empty;

            if (string.IsNullOrEmpty(inputDateString) && string.IsNullOrEmpty(defaultDateString))
                return outputDateTimeString;

            CultureInfo inputCultureInfo;
            if (string.IsNullOrEmpty(inputCultureInfoName))
                if (inputDateString.Contains("/"))
                    inputCultureInfo = new CultureInfo("en-GB");
                else
                    inputCultureInfo = CultureInfo.InvariantCulture;
            else
                inputCultureInfo = new CultureInfo(inputCultureInfoName);

            CultureInfo outputCultureInfo;
            if (string.IsNullOrEmpty(outputCultureInfoName))
                outputCultureInfo = CultureInfo.InvariantCulture;
            else
                outputCultureInfo = new CultureInfo(outputCultureInfoName);

            if (DateTime.TryParseExact(inputDateString, inputDateFormatArray, inputCultureInfo, DateTimeStyles.None, out outputDateTime))
            {
                outputDateTimeString = outputDateTime.ToString(outputDateFormat, outputCultureInfo);
                return outputDateTimeString;
            }
            else
            {
                Log.Info(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { inputDateString, defaultDateString, inputDateFormat, outputDateFormat, inputCultureInfoName, outputCultureInfoName }, "DateTime.TryParseExact\n\rFirst Level Warning: {0}", new object[] { string.Format("inputDateString '{0}' is not in an acceptable format.", new object[] { inputDateString }) }));

                try
                {
                    outputDateTime = DateTime.Parse(inputDateString, inputCultureInfo, DateTimeStyles.None);
                    outputDateTimeString = outputDateTime.ToString(outputDateFormat, outputCultureInfo);

                    return outputDateTimeString;
                }
                catch (Exception ex)
                {
                    Log.Info(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { inputDateString, defaultDateString, inputDateFormat, outputDateFormat, inputCultureInfoName, outputCultureInfoName }, "DateTime.Parse\n\rSecond Level Warning: {0}", new object[] { ex.Message }));
                }
            }

            if (DateTime.TryParseExact(defaultDateString, inputDateFormatArray, inputCultureInfo, DateTimeStyles.None, out outputDateTime))
            {
                outputDateTimeString = outputDateTime.ToString(outputDateFormat, outputCultureInfo);
                return outputDateTimeString;
            }
            else
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { inputDateString, defaultDateString, inputDateFormat, outputDateFormat, inputCultureInfoName, outputCultureInfoName }, "DateTime.TryParseExact\n\rError: {0}", new object[] { string.Format("defaultDateString '{0}' is not in an acceptable format.", new object[] { defaultDateString }) }));

            outputDateTimeString = DateTime.MinValue.ToString(outputDateFormat, outputCultureInfo);

            return outputDateTimeString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputTimeString"></param>
        /// <param name="defaultTimeString"></param>
        /// <param name="inputTimeFormat"></param>
        /// <param name="outputTimeFormat"></param>
        /// <param name="inputCultureInfoName"></param>
        /// <param name="outputCultureInfoName"></param>
        /// <returns></returns>
        static public string FormatTime(string inputTimeString, string defaultTimeString, string inputTimeFormat, string outputTimeFormat, string inputCultureInfoName, string outputCultureInfoName)
        {
            Log.DebugFormat("FormatTime(string inputTimeString {0}, string defaultTimeString {1}, string inputTimeFormat {2}, string outputTimeFormat {3}, string inputCultureInfoName {4}, string outputCultureInfoName {5})", inputTimeString, defaultTimeString, inputTimeFormat, outputTimeFormat, inputCultureInfoName, outputCultureInfoName);

            DateTime outputTime;
            string outputTimeString = string.Empty;

            if (string.IsNullOrEmpty(inputTimeString) && string.IsNullOrEmpty(defaultTimeString))
                return outputTimeString;

            CultureInfo inputCultureInfo;
            if (string.IsNullOrEmpty(inputCultureInfoName))
                inputCultureInfo = CultureInfo.InvariantCulture;
            else
                inputCultureInfo = new CultureInfo(inputCultureInfoName);

            CultureInfo outputCultureInfo;
            if (string.IsNullOrEmpty(outputCultureInfoName))
                outputCultureInfo = CultureInfo.InvariantCulture;
            else
                outputCultureInfo = new CultureInfo(outputCultureInfoName);

            if (DateTime.TryParseExact(inputTimeString, inputTimeFormat, inputCultureInfo, DateTimeStyles.None, out outputTime))
            {
                outputTimeString = outputTime.ToString(outputTimeFormat, outputCultureInfo);
                return outputTimeString;
            }
            else
            {
                Log.Warn(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { inputTimeString, defaultTimeString, inputTimeFormat, outputTimeFormat, inputCultureInfoName, outputCultureInfoName }, "Time.TryParseExact\n\rFirst Level Warning: {0}", new object[] { string.Format("inputTimeString '{0}' is not in an acceptable format.", new object[] { inputTimeString }) }));

                try
                {
                    outputTime = DateTime.Parse(inputTimeString, inputCultureInfo, DateTimeStyles.None);
                    outputTimeString = outputTime.ToString(outputTimeFormat, outputCultureInfo);

                    return outputTimeString;
                }
                catch (Exception ex)
                {
                    Log.Warn(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { inputTimeString, defaultTimeString, inputTimeFormat, outputTimeFormat, inputCultureInfoName, outputCultureInfoName }, "Time.Parse\n\rSecond Level Warning: {0}", new object[] { ex.Message }));
                }
            }

            if (DateTime.TryParseExact(defaultTimeString, inputTimeFormat, inputCultureInfo, DateTimeStyles.None, out outputTime))
            {
                outputTimeString = outputTime.ToString(outputTimeFormat, outputCultureInfo);
                return outputTimeString;
            }
            else
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { inputTimeString, defaultTimeString, inputTimeFormat, outputTimeFormat, inputCultureInfoName, outputCultureInfoName }, "Time.TryParseExact\n\rError: {0}", new object[] { string.Format("defaultTimeString '{0}' is not in an acceptable format.", new object[] { defaultTimeString }) }));

            outputTimeString = DateTime.MinValue.ToString(outputTimeFormat, outputCultureInfo);

            return outputTimeString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="defaultDateString"></param>
        /// <param name="defaultTimeString"></param>
        /// <param name="inputDateFormat"></param>
        /// <param name="outputTimeFormat"></param>
        /// <param name="cultureInfoName"></param>
        /// <returns></returns>
        static public string FormatTimeFromSecondsAfterMidnight(double seconds, string defaultDateString, string defaultTimeString, string inputDateFormat, string outputTimeFormat, string cultureInfoName)
        {
            Log.DebugFormat("FormatTimeFromSecondsAfterMidnight(double seconds {0}, string defaultDateString {1}, string defaultTimeString {2}, string inputDateFormat {3}, string outputTimeFormat {4}, string cultureInfoName {5})", seconds, defaultDateString, defaultTimeString, inputDateFormat, outputTimeFormat, cultureInfoName);

            DateTime outputDateTime;
            string outputDateTimeString = string.Empty;

            CultureInfo cultureInfo;
            if (string.IsNullOrEmpty(cultureInfoName))
                cultureInfo = CultureInfo.InvariantCulture;
            else
                cultureInfo = new CultureInfo(cultureInfoName);

            if (DateTime.TryParseExact(defaultDateString, inputDateFormat, cultureInfo, DateTimeStyles.None, out outputDateTime))
            {
                TimeSpan tsAfterMidnight = TimeSpan.FromSeconds(seconds);
                outputDateTime = outputDateTime.Add(tsAfterMidnight);
                outputDateTimeString = outputDateTime.ToString(outputTimeFormat);
                return outputDateTimeString;
            }
            else
                Log.Warn(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { seconds, defaultDateString, defaultTimeString, inputDateFormat, outputTimeFormat, cultureInfoName }, "DateTime.TryParseExact\n\rFirst Level Exception: {0}", new object[] { string.Format("defaultDateString '{0}' is not in an acceptable format.", defaultDateString) }));

            try
            {
                outputDateTime = DateTime.MinValue;
                TimeSpan tsAfterMidnight = TimeSpan.FromSeconds(seconds);
                outputDateTime = outputDateTime.Add(tsAfterMidnight);
                outputDateTimeString = outputDateTime.ToString(outputTimeFormat);
            }
            catch (Exception ex)
            {
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { seconds, defaultDateString, defaultTimeString, inputDateFormat, outputTimeFormat, cultureInfoName }, "Second Level Exception: {0}", new object[] { ex.Message }));
                outputDateTimeString = defaultTimeString;
            }

            return outputDateTimeString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="defaultOutputValue"></param>
        /// <param name="outputNumberFormat"></param>
        /// <param name="inputCultureInfoName"></param>
        /// <param name="outputCultureInfoName"></param>
        /// <returns></returns>
        static public string FormatNumber(string inputValue, string defaultOutputValue, string outputNumberFormat, string inputCultureInfoName, string outputCultureInfoName)
        {
            Log.DebugFormat("FormatNumber(string inputValue {0}, string defaultOutputValue {1}, string outputNumberFormat {2}, string inputCultureInfoName {3}, string outputCultureInfoName {4})", inputValue, defaultOutputValue, outputNumberFormat, inputCultureInfoName, outputCultureInfoName);

            try
            {
                inputValue = Regex.Match(inputValue, @"[+-]?\d+(\.\d+)?(\,\d+)?").Value;
            }
            catch (Exception ex)
            {
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { inputValue, defaultOutputValue, inputCultureInfoName, outputCultureInfoName, outputNumberFormat }, ex));
            }

            CultureInfo inputCultureInfo;
            if (string.IsNullOrEmpty(inputCultureInfoName))
            {
                inputValue = inputValue.Replace(',', '.');
                inputCultureInfo = CultureInfo.InvariantCulture;
            }
            else
                inputCultureInfo = new CultureInfo(inputCultureInfoName);

            CultureInfo outputCultureInfo;
            if (string.IsNullOrEmpty(outputCultureInfoName))
                outputCultureInfo = CultureInfo.InvariantCulture;
            else
                outputCultureInfo = new CultureInfo(outputCultureInfoName);

            decimal outputNumber;

            if (Decimal.TryParse(inputValue, NumberStyles.Number, inputCultureInfo, out outputNumber))
            {
                return string.Format(outputCultureInfo, string.Format("{{0:{0}}}", outputNumberFormat), outputNumber);
            }
            else if (Decimal.TryParse(defaultOutputValue, NumberStyles.Number, inputCultureInfo, out outputNumber))
            {
                return string.Format(outputCultureInfo, string.Format("{{0:{0}}}", outputNumberFormat), outputNumber);
            }

            Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { inputValue, defaultOutputValue, inputCultureInfoName, outputCultureInfoName, outputNumberFormat }, "Error: {0}", new object[] { "Cannot parse value" }));

            return string.Format(outputCultureInfo, outputNumberFormat, Decimal.MinValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="defaultOutputString"></param>
        /// <param name="inputTrueString"></param>
        /// <param name="inputFalseString"></param>
        /// <param name="outputTrueString"></param>
        /// <param name="outputFalseString"></param>
        /// <returns></returns>
        static public string FormatBool(string inputValue, string defaultOutputString, string inputTrueString, string inputFalseString, string outputTrueString, string outputFalseString)
        {
            Log.DebugFormat("FormatBool(string inputValue {0}, string defaultOutputString {1}, string inputTrueString {2}, string inputFalseString {3}, string outputTrueString {4}, string outputFalseString {5})", inputValue, defaultOutputString, inputTrueString, inputFalseString, outputTrueString, outputFalseString);

            if (string.IsNullOrEmpty(inputValue))
                return defaultOutputString;

            int iValue;
            bool bValue;

            if (int.TryParse(inputValue, out iValue))
            {
                if (iValue < 1)
                    return outputTrueString;
                else
                    return outputFalseString;
            }
            else if (bool.TryParse(inputValue, out bValue))
            {
                if (bValue)
                    return outputTrueString;
                else
                    return outputFalseString;
            }
            else if (inputValue == inputTrueString)
            {
                return outputTrueString;
            }
            else if (inputValue == inputFalseString)
            {
                return outputFalseString;
            }

            Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { inputValue, defaultOutputString, inputTrueString, inputFalseString, outputTrueString, outputFalseString }, "Error: {0}", new object[] { "Cannot parse value" }));

            return defaultOutputString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public string GenerateGuid()
        {
            Log.DebugFormat("GenerateGuid()");

            return System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="convertValue"></param>
        /// <returns></returns>
        static public string ConvertEmptyValue(string inputValue, string convertValue)
        {
            Log.DebugFormat("ConvertEmptyValue(string inputValue {0}, string convertValue {1})", inputValue, convertValue);

            if (string.IsNullOrEmpty(inputValue))
                return convertValue;
            else
                return inputValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="controlCharacterDecimal"></param>
        /// <param name="substitutionCharacterDecimal"></param>
        /// <returns></returns>
        static public string ReplaceCharacterByDecimal(string inputValue, byte controlCharacterDecimal, byte substitutionCharacterDecimal)
        {
            Log.DebugFormat("ReplaceCharacterByDecimal(string inputValue {0}, byte controlCharacterDecimal {1}, byte substitutionCharacterDecimal {2})", inputValue, controlCharacterDecimal, substitutionCharacterDecimal);

            string modifiedValue = string.Empty;

            if (string.IsNullOrEmpty(inputValue))
                return inputValue;

            try
            {
                modifiedValue = inputValue.Replace((char)controlCharacterDecimal, (char)substitutionCharacterDecimal);
            }
            catch (Exception ex)
            {
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { inputValue, controlCharacterDecimal, substitutionCharacterDecimal }, ex));
            }

            return modifiedValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        static public string ReplaceString(string inputValue, string oldValue, string newValue)
        {
            Log.DebugFormat("ReplaceString(string inputValue {0}, string oldValue {1}, string newValue {2})", inputValue, oldValue, newValue);

            string modifiedValue = string.Empty;

            if (string.IsNullOrEmpty(inputValue))
                return inputValue;

            try
            {
                modifiedValue = inputValue.Replace(oldValue, newValue);
            }
            catch (Exception ex)
            {
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { inputValue, oldValue, newValue }, ex));
            }

            return modifiedValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="replaceValue"></param>
        /// <returns></returns>
        static public string ReplaceNewLine(string inputValue, string replaceValue)
        {
            Log.DebugFormat("ReplaceNewLine(string inputValue {0}, string replaceValue {1})", inputValue, replaceValue);

            string modifiedValue = string.Empty;

            if (string.IsNullOrEmpty(inputValue))
                return inputValue;

            try
            {
                modifiedValue = inputValue.Replace(Environment.NewLine, replaceValue);
            }
            catch (Exception ex)
            {
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { inputValue, replaceValue }, ex));
            }

            return modifiedValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="replaceValue"></param>
        /// <returns></returns>
        static public string ReplaceNullOrEmpty(string inputValue, string replaceValue)
        {
            Log.DebugFormat("ReplaceNullOrEmpty(string inputValue {0}, string replaceValue {1})", inputValue, replaceValue);

            if (string.IsNullOrEmpty(inputValue))
                return replaceValue;

            return inputValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputValue"></param>
        /// <returns></returns>
        static public string Anonymize(string inputValue)
        {
            Log.DebugFormat("Anonymize(string inputValue {0})", inputValue);

            int RealCharLeft = 0;
            string AnonymizeCharacters = "nv";

            try
            {
                if (String.IsNullOrEmpty(inputValue) || Configuration.GetNumericAppSettingsValue("Anonymize") != 1)
                    return inputValue;

                RealCharLeft = Convert.ToInt32(Configuration.GetNumericAppSettingsValue("RealCharLength"));
                AnonymizeCharacters = Configuration.GetTextAppSettingsValue("AnonymizeCharacters");

                if (inputValue.Length < RealCharLeft)
                    return inputValue;

                return string.Concat(inputValue.Substring(0, RealCharLeft), AnonymizeCharacters);
            }
            catch (Exception ex)
            {
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { inputValue }, ex));

                return "ERROR - Anonymize";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="separator"></param>
        /// <param name="notEqualValue"></param>
        /// <param name="stripLastSeparator"></param>
        /// <returns></returns>
        static public string CheckEqualArrayElements(string inputValue, string separator, string notEqualValue, bool stripLastSeparator)
        {
            Log.DebugFormat("CheckEqualArrayElements(string inputValue {0}, string separator {1}, string notEqualValue {2}, bool stripLastSeparator {3})", inputValue, separator, notEqualValue, stripLastSeparator);

            if (stripLastSeparator)
                inputValue = inputValue.Substring(0, inputValue.Length - 1);

            string[] transferStatusArray = inputValue.Split(Convert.ToChar(separator));

            int i;
            for (i = 1; i < transferStatusArray.Length; i++)
            {
                if (transferStatusArray[0] == transferStatusArray[i])
                    continue;

                else
                    break;
            }
            if (i == transferStatusArray.Length)
                return transferStatusArray[0];
            else
                return notEqualValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="trueValue"></param>
        /// <param name="falseValue"></param>
        /// <returns></returns>
        static public string TransformBooleanString(string inputValue, string trueValue, string falseValue)
        {
            switch (inputValue.ToLower())
            {
                case "j":
                case "ja":
                case "y":
                case "yes":
                case "true":
                case "1":
                    return trueValue;
                case "n":
                case "nein":
                case "no":
                case "none":
                case "false":
                case "0":
                    return falseValue;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public string IdFromTimestamp()
        {
            return DateTime.Now.ToFileTimeUtc().ToString();
        }

    }
}
