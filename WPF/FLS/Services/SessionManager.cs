using System;

namespace FLS.Services
{
    public class SessionManager
    {
        private static SessionManager? _instance;
        private static readonly object _lock = new object();

        public Guid? CurrentUserId { get; private set; }
        public string? CurrentUserName { get; private set; }
        public string? CurrentUserEmail { get; private set; }
        public bool IsAdmin { get; private set; }

        private SessionManager() { }

        public static SessionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SessionManager();
                        }
                    }
                }
                return _instance;
            }
        }

        public void Login(Guid userId, string userName, string email, bool isAdmin = false)
        {
            CurrentUserId = userId;
            CurrentUserName = userName;
            CurrentUserEmail = email;
            IsAdmin = isAdmin;
        }

        public void Logout()
        {
            CurrentUserId = null;
            CurrentUserName = null;
            CurrentUserEmail = null;
            IsAdmin = false;
        }

        public bool IsLoggedIn()
        {
            return CurrentUserId.HasValue;
        }

        public Guid GetCurrentUserId()
        {
            if (!CurrentUserId.HasValue)
            {
                throw new InvalidOperationException("No user is currently logged in.");
            }
            return CurrentUserId.Value;
        }
    }
}
