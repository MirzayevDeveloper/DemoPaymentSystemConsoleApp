using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoPaymentSystem.Domains
{
    public class HistoryModel
    {

        private User _sender, _receiver;
        private string _transactionId;
        private decimal _amount;

        public decimal ServicePrice { get; set; }

        public required User Sender
        {
            get { return _sender; }
            set { _sender = value; }
        }
        public User Receiver
        {
            get { return _receiver; }
            set { _receiver = value; }
        }

        public decimal Amount
        {
            get { return _amount; }
            set { _amount = value; }
        }

        public string TransactionId { get => _transactionId; private init => _transactionId = value; }

        public HistoryModel()
        {
            _transactionId = Guid.NewGuid().ToString();
        }
        public override string ToString()
        {
            return $"Id: {Sender.Id}\nSenderName: {Sender.Owner}\nSender: {CardEncrypt(Sender.CardNumber)}\n" +
                    $"ReceiverName: {Receiver.Owner}\nReceiver: {CardEncrypt(Receiver.CardNumber)}\n" +
                    $"ServicePrice: {ServicePrice}\nAmountOfTransaction: {Amount}\nDateTime: {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}\n" +
                    $"TransactionId: {Guid.NewGuid()}\n";
        }
        private string CardEncrypt(string? card)
        {
            string? _card = card.Substring(0, 6);
            _card += "******";
            _card += card.Substring(card.Length - 4, 4);
            return _card;
        }


        
    }
}
