using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Http
{
    public static class HttpRequestExtensions
    {
        public static string GetClientIpAddress(this HttpRequest request)
        {
            return GetClientIpAddress(request.HttpContext.Connection.RemoteIpAddress.ToString(), request.Headers["X-Forwarded-For"]);
        }

        // Reference: http://stackoverflow.com/questions/2577496/how-can-i-get-the-clients-ip-address-in-asp-net-mvc
        public static string GetClientIpAddress(string userHostAddress, string xForwardedFor)
        {
            try
            {
                // We do not want to deal with ipv6 localhost addresses as none of the system, including
                // external systems like SagePay are set up to handle them correctly as of 12/04/2016
                if (userHostAddress == "::1")
                {
                    return "127.0.0.1";
                }

                if (string.IsNullOrEmpty(xForwardedFor))
                {
                    // Attempt to parse.  If it fails, we catch below and return "0.0.0.0"
                    // Could use TryParse instead, but I wanted to catch all exceptions
                    IPAddress.Parse(userHostAddress);

                    return userHostAddress;
                }

                // Get a list of public ip addresses in the X-Forwarded-For header
                var publicForwardingIps = xForwardedFor
                    .Split(',')
                    .Select(StripPort)
                    .Where(ip => !IsPrivateIpAddress(ip))
                    .ToList();

                // If we found any, return the last one, otherwise return the user host address
                return publicForwardingIps.Any() ? publicForwardingIps.First() : userHostAddress;
            }
            catch (Exception)
            {
                return "0.0.0.0";
            }
        }

        private static string StripPort(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                return ipAddress;
            }

            ipAddress = ipAddress.Trim();

            var colonIndex = ipAddress.LastIndexOf(':');

            return colonIndex == -1 ? ipAddress : ipAddress.Substring(0, colonIndex);
        }

        private static bool IsPrivateIpAddress(string ipAddress)
        {
            // http://en.wikipedia.org/wiki/Private_network
            // Private IP Addresses are:
            //  24-bit block: 10.0.0.0 through 10.255.255.255
            //  20-bit block: 172.16.0.0 through 172.31.255.255
            //  16-bit block: 192.168.0.0 through 192.168.255.255
            //  Link-local addresses: 169.254.0.0 through 169.254.255.255 (http://en.wikipedia.org/wiki/Link-local_address)

            var ip = IPAddress.Parse(ipAddress);
            var octets = ip.GetAddressBytes();

            var is24BitBlock = octets[0] == 10;
            if (is24BitBlock)
            {
                return true;
            }

            var is20BitBlock = octets[0] == 172 && octets[1] >= 16 && octets[1] <= 31;
            if (is20BitBlock)
            {
                return true;
            }

            var is16BitBlock = octets[0] == 192 && octets[1] == 168;
            if (is16BitBlock)
            {
                return true;
            }

            var isLinkLocalAddress = octets[0] == 169 && octets[1] == 254;
            return isLinkLocalAddress;
        }
    }
}
