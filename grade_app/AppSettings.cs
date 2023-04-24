using Grade;
using System;
using System.Collections.Generic;
using System.Text;

namespace grade_app
{
    class AppSettings
    {
		public AppSettings(string token, Role role)
		{
			this.token = token;
			this.role = role;
		}

		public string token { get; private set; }
        public Grade.Role role { get; private set; }
    }
}
