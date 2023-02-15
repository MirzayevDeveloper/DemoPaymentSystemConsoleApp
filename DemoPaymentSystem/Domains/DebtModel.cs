using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoPaymentSystem.Domains
{
    public class DebtModel : ICloneable
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public decimal debtAmount { get; set; }

        public object Clone()
        {
            return new DebtModel
            {
                UserId = this.UserId,
                UserName = this.UserName,
                debtAmount = this.debtAmount
            };
        }
    }
}
