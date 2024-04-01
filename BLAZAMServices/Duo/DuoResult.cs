using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services.Duo
{
    public class AccessDevice
    {
        public object browser { get; set; }
        public object browser_version { get; set; }
        public object flash_version { get; set; }
        public object hostname { get; set; }
        public string ip { get; set; }
        public object is_encryption_enabled { get; set; }
        public object is_firewall_enabled { get; set; }
        public object is_password_set { get; set; }
        public object java_version { get; set; }
        public Location location { get; set; }
        public object os { get; set; }
        public object os_version { get; set; }
    }

    public class Application
    {
        public string key { get; set; }
        public string name { get; set; }
    }

    public class AuthContext
    {
        public AccessDevice access_device { get; set; }
        public string alias { get; set; }
        public Application application { get; set; }
        public AuthDevice auth_device { get; set; }
        public string email { get; set; }
        public string event_type { get; set; }
        public string factor { get; set; }
        public DateTime isotimestamp { get; set; }
        public object ood_software { get; set; }
        public string reason { get; set; }
        public string result { get; set; }
        public int timestamp { get; set; }
        public string trusted_endpoint_status { get; set; }
        public string txid { get; set; }
        public User user { get; set; }
    }

    public class AuthDevice
    {
        public object ip { get; set; }
        public Location location { get; set; }
        public object name { get; set; }
    }

    public class AuthResult
    {
        public string result { get; set; }
        public string status { get; set; }
        public string status_msg { get; set; }
    }

    public class Location
    {
        public string city { get; set; }
        public string country { get; set; }
        public string state { get; set; }
    }

    public class Root
    {
        public AuthContext AuthContext { get; set; }
        public AuthResult AuthResult { get; set; }
        public int AuthTime { get; set; }
        public string Username { get; set; }
        public string Iss { get; set; }
        public DateTime Exp { get; set; }
        public DateTime Iat { get; set; }
        public string Sub { get; set; }
        public string Aud { get; set; }
        public object Nonce { get; set; }
    }

    public class User
    {
        public List<string> groups { get; set; }
        public string key { get; set; }
        public string name { get; set; }
    }

}
