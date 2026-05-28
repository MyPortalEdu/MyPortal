namespace MyPortal.Auth.Constants
{
    public class ClaimTypes
    {
        public const string UserType = "ut";
        public const string UserSecurityStamp = "usrver";

        // The Person the user is linked to (User.PersonId). Absent for users with no person
        // identity (service accounts). Used to resolve self / line-management relationships.
        public const string PersonId = "pid";
    }
}
