using DemoPaymentSystem.States;

namespace DemoPaymentSystem.Domains
{
    public class User
    {
        private string? _name, _cardNumber, _phoneNumber, _password;
        private Guid _guid;
        private decimal _balance;
        public Guid Id { get => _guid; private set => _guid = value; }
        public string? Owner { get => _name; set => _name = value; }
        public string? Phone { get => _phoneNumber; set => _phoneNumber = value; }
        public string? CardNumber { get => _cardNumber; private init => _cardNumber = value; }
        public UserStatus AccountStatus { get; set; }
        public CardType TypeOfCard { get; private init; }
        public decimal Balance { get => _balance; set => _balance = value; }
        public string? Password { get => _password; set => _password = value; }

        public override string ToString() => $"Id: {Id}\n{TypeOfCard}CardNumber: {CardNumber}\n" +
            $"Balance: {Balance}\nOwner: {Owner}\nPhone: {Phone}\nPassword: {Password}\nAccountStatus: {AccountStatus}\n";
        
        public User(string? cardNumber, Guid? guid)
        {
            Id = guid ?? Guid.NewGuid();
            CardNumber = cardNumber;
            string? valid = CardNumber?.Substring(0, 4);

            TypeOfCard = valid switch
            {
                "8600" => CardType.Uzcard,
                "9860" => CardType.Humo,
                "4200" => CardType.Visa,
                "5600" => CardType.MasterCard,
                _ => CardType.None,
            };
        }
        
        public User()
        {

        }
    }
}
