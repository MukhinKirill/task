namespace TR.Connector
{
    public partial class Connector
    {
        //-------TokenResponse------------//
        class TokenResponseData
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
        }

        class TokenResponse
        {
            public TokenResponseData data { get; set; }
            public bool success { get; set; }
            public object errorText { get; set; }
            public object count { get; set; }
        }
        //-------TokenResponse------------//

        //-------RoleResponse------------//
        class RoleResponseData
        {
            public int id { get; set; }
            public string name { get; set; }
            public string corporatePhoneNumber { get; set; }
        }

        class RoleResponse
        {
            public List<RoleResponseData> data { get; set; }
            public bool success { get; set; }
            public object errorText { get; set; }
            public int count { get; set; }
        }
        //-------RoleResponse------------//

        //-------RightResponse------------//
        class RightResponseData
        {
            public int id { get; set; }
            public string name { get; set; }
            public object users { get; set; }
        }

        class RightResponse
        {
            public List<RightResponseData> data { get; set; }
            public bool success { get; set; }
            public object errorText { get; set; }
            public int count { get; set; }
        }
        //-------RightResponse------------//


        //-------UserRoleResponse------------//
        class UserRoleResponse
        {
            public List<RoleResponseData> data { get; set; }
            public bool success { get; set; }
            public object errorText { get; set; }
            public int count { get; set; }
        }
        //-------UserRoleResponse------------//

        //-------UserRoleResponse------------//
        class UserrightResponse
        {
            public List<RightResponseData> data { get; set; }
            public bool success { get; set; }
            public object errorText { get; set; }
            public int count { get; set; }
        }
        //-------UserRoleResponse------------//


        //-------UserResponse------------//
        class UserResponseData
        {
            public string login { get; set; }
            public string status { get; set; }
        }

        class UserResponse
        {
            public List<UserResponseData> data { get; set; }
            public bool success { get; set; }
            public object errorText { get; set; }
            public int count { get; set; }
        }
        //-------UserResponse------------//

        //-------UserPropertyResponse------------//
        class UserPropertyData
        {
            public string lastName { get; set; }
            public string firstName { get; set; }
            public string middleName { get; set; }
            public string telephoneNumber { get; set; }
            public bool isLead { get; set; }
            public string login { get; set; }
            public string status { get; set; }
        }

        class UserPropertyResponse
        {
            public UserPropertyData data { get; set; }
            public bool success { get; set; }
            public object errorText { get; set; }
            public int count { get; set; }
        }

        class CreateUSerDTO : UserPropertyData
        {
            public string password { get; set; }
        }
        //-------UserPropertyResponse------------//
    }
}
