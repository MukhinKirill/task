using System.Text.Json.Serialization;
using Task.Integration.Data.Models.Models;

namespace Connector.Infrastructure.DataAccess.Models.POCO
{
    public class UserPOCO
    {
        #region Private

        private string login = "";
        private string firstName = "";
        private string lastName = "";
        private string middleName = "";
        private string telephoneNumber = "";
        private bool isLead = false;

        #endregion

        public UserPOCO() { }

        public UserPOCO(string login, IEnumerable<UserProperty> properties)
        {
            this.login = login;
            foreach (var property in properties)
            {
                switch (property.Name)
                {
                    case nameof(firstName):
                        firstName = property.Value;
                        break;
                    case nameof(lastName):
                        lastName = property.Value;
                        break;
                    case nameof(middleName):
                        middleName = property.Value;
                        break;
                    case nameof(telephoneNumber):
                        telephoneNumber = property.Value;
                        break;
                    case nameof(isLead):
                        isLead = property.Value == "true";
                        break;
                }
            }
        }

        #region Properties

        /// <summary>
        /// Логин
        /// </summary>
        [JsonPropertyName("login")]
        public string Login
        {
            get
            {
                return login;
            }
            set
            {
                login = value;
            }
        }

        /// <summary>
        /// Имя
        /// </summary>
        [JsonPropertyName("firstName")]
        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {
                firstName = value;
            }
        }

        /// <summary>
        /// Фамилия
        /// </summary>
        [JsonPropertyName("lastName")]
        public string LastName
        {
            get
            {
                return lastName;
            }
            set
            {
                lastName = value;
            }
        }

        /// <summary>
        /// Отчество
        /// </summary>
        [JsonPropertyName("middleName")]
        public string MiddleName
        {
            get
            {
                return middleName;
            }
            set
            {
                middleName = value;
            }
        }

        /// <summary>
        /// Телефонный номер
        /// </summary>
        [JsonPropertyName("telephoneNumber")]
        public string TelephoneNumber
        {
            get
            {
                return telephoneNumber;
            }
            set
            {
                telephoneNumber = value;
            }
        }

        /// <summary>
        /// Начальник
        /// </summary>
        [JsonPropertyName("isLead")]
        public bool IsLead
        {
            get
            {
                return isLead;
            }
            set
            {
                isLead = value;
            }
        }

        #endregion

        #region Methods

        public IEnumerable<UserProperty> GetProperty()
        {
            return new List<UserProperty>
            {
                new UserProperty(nameof(firstName), firstName),
                new UserProperty(nameof(lastName), lastName),
                new UserProperty(nameof(middleName), middleName),
                new UserProperty(nameof(telephoneNumber), telephoneNumber),
                new UserProperty(nameof(isLead), isLead.ToString())
            };
        }

        #endregion
    }
}
