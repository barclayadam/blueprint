using System.Text.RegularExpressions;

namespace Blueprint.Utilities
{
    /// <summary>
    /// Provides a number of regular expressions for common scenarios such as postcodes and email
    /// addresses, ensuring that projects can use these expressions knowing they have already been
    /// tested and to avoid littering many projects with many expressions, usually all slightly
    /// modified.
    /// </summary>
    public static class CommonRegularExpressions
    {
        /// <summary>
        /// A regular expression pattern which represents an email address, attempting to filter our the majority
        /// of incorrect email addresses whilst avoiding false-negatives.
        /// </summary>
        /// <remarks>
        /// Although this pattern will catch the vast majority of bad email addresses it can not be relied upon
        /// for validating the email address is actually correct, only sending an email can do that.
        /// </remarks>
        public const string EmailPattern =
                @"[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*" + // Username
                "@" + // Separator
                @"(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?"; // Domain / IP Address part

        /// <summary>
        /// A regular expression pattern which attempts to match a phone number.
        /// </summary>
        public const string PhoneNumberPattern = @"[\d .()+x-]{1,30}";

        /// <summary>
        /// Attempts to match a UK postcode, eliminating many of the invalid postcodes which do not follow patterns set
        /// out by postal service, such as the valid characters in certain positions.
        /// </summary>
        /// <remarks>
        /// As with validating email addresses a postcode that matches this regular expression does not guarantee the
        /// postcode is valid, only other checks could verify this.
        /// </remarks>
        public const string UkPostcodePattern =
                @"([Gg][Ii][Rr] ?0[aA]{2})|((([a-zA-Z][0-9]{1,2})|(([a-zA-Z][A-HJ-Ya-hj-y][0-9]{1,2})|(([a-zA-Z][0-9][a-zA-Z])|([a-zA-Z][A-HJ-Ya-hj-y][0-9]?[a-zA-Z])))) ?[0-9][a-zA-Z]{2})";

        /// <summary>
        /// A regular expression pattern which attempts to match a Url, one which is either an FTP or HTTP (or no protocol), either plain
        /// or secure (HTTPS / FTPS), with optional path and query string.
        /// </summary>
        public const string UrlPattern = UrlProtocolPattern + "?" + UrlDomainPattern;

        // Original Source: http://www.regular-expressions.info/email.html

        /// <summary>
        /// A regular expression that can be used to find email addresses within a string, using word
        /// boundaries to separate an email address from the surrounding text.
        /// </summary>
        /// <remarks>
        /// Although this pattern will catch the vast majority of bad email addresses it can not be relied upon
        /// for validating the email address is actually correct,only sending an email can do that.
        /// </remarks>
        public static readonly Regex Email = new Regex(
                $@"\b{EmailPattern}\b", RegexOptions.Compiled);

        /// <summary>
        /// A regular expression to determine whether or not a string matches an email and nothing else,
        /// cannot be used to find emails within other text.
        /// </summary>
        /// <remarks>
        /// Although this pattern will catch the vast majority of bad email addresses it can not be relied upon
        /// for validating the email address is actually correct,only sending an email can do that.
        /// </remarks>
        public static readonly Regex EmailOnly = new Regex(
                $@"^\s*{EmailPattern}\s*$", RegexOptions.Compiled);

        /// <summary>
        /// A regular expression that can be used to find phone numbers within a string, using word
        /// boundaries to separate a phone number from the surrounding text.
        /// </summary>
        public static readonly Regex PhoneNumber = new Regex(
                $@"\b{PhoneNumberPattern}\b", RegexOptions.Compiled);

        /// <summary>
        /// A regular expression to determine whether or not a string matches a phone number and nothing else,
        /// cannot be used to find phone numbers within other text.
        /// </summary>
        public static readonly Regex PhoneNumberOnly = new Regex(
                $@"^\s*{PhoneNumberPattern}\s*$", RegexOptions.Compiled);

        /// <summary>
        /// A regular expression that can be used to find UK postcodes within a string, using word
        /// boundaries to separate an email address from the surrounding text.
        /// </summary>
        public static readonly Regex UkPostcode = new Regex(
                $@"\b{UkPostcodePattern}\b", RegexOptions.Compiled);

        /// <summary>
        /// A regular expression to determine whether or not a string matches a UK postcode and nothing else,
        /// cannot be used to find emails within other text.
        /// </summary>
        public static readonly Regex UkPostcodeOnly = new Regex(
                $@"^\s*{UkPostcodePattern}\s*$", RegexOptions.Compiled);

        /// <summary>
        /// A regular expression that can be used to find URLs within a string, using word
        /// boundaries to separate an email address from the surrounding text.
        /// </summary>
        public static readonly Regex Url = new Regex(
                $@"\b{UrlPattern}\b", RegexOptions.Compiled);

        /// <summary>
        /// A regular expression to determine whether or not a string matches a URL and nothing else,
        /// cannot be used to find emails within other text.
        /// </summary>
        public static readonly Regex UrlOnly = new Regex(
                $@"^\s*{UrlPattern}\s*$", RegexOptions.Compiled);

        /// <summary>
        /// A regular expression to determine whether or not a string matches a URL and nothing else,
        /// cannot be used to find emails within other text.
        /// </summary>
        public static readonly Regex UrlWithProtocol = new Regex(
            $@"^\s*{UrlWithProtocolPattern}\s*$", RegexOptions.Compiled);

        private const string UrlProtocolPattern = @"((([Hh][Tt]|[Ff])[Tt][Pp]([Ss]?))\://)";
        private const string UrlWithProtocolPattern = UrlProtocolPattern + UrlDomainPattern;
        private const string UrlDomainPattern = @"([a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,63}|localhost)(\:[0-9]{1,5})*([/?]($|[a-zA-Z0-9\.\,\;\'\\\+&amp;()%\$#\=~_\-]+))*";
    }
}
