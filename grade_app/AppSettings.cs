using Grade;
using System;
using System.Collections.Generic;
using System.Text;

namespace grade_app
{
    class AppSettings(string token, Role role)
    {
        public string token { get; private set; } = token;
        public Grade.Role role { get; private set; } = role;
    }
}
