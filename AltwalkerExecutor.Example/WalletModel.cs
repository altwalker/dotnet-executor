//    Copyright(C) 2023 Altom Consulting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace Altwalker.Executor
{
    public class WalletModel
    {
        Wallet wallet;

        public void setUpModel()
        {
            wallet = new Wallet(1000);
        }

        #region vertices
        public void full_wallet(IDictionary<string, dynamic> data)
        {
            data["amount"] = wallet.GetAmount();
            Assert.AreEqual(1000, wallet.GetAmount());
        }

        public void non_full_wallet(IDictionary<string, dynamic> data)
        {
            Assert.AreEqual(wallet.GetAmount(), int.Parse(data["amount"]));
            Assert.IsTrue(true);
        }
        #endregion

        #region edges
        public void pay_random_amount(IDictionary<string, dynamic> data)
        {
            Random r = new Random();
            int toPay = r.Next() % wallet.GetAmount();

            wallet.Pay(toPay);

            data["amount"] = wallet.GetAmount();
            Trace.WriteLine($"payed {toPay}");

        }
        #endregion
    }

    public class Setup
    {
        public void SetupRun()
        {
        }
    }

    public class Wallet
    {
        private int amount;

        public Wallet(int amount)
        {
            this.amount = amount;
        }

        public void Pay(int cost)
        {
            this.amount -= cost;
        }

        public int GetAmount()
        {
            return amount;
        }
    }
}
