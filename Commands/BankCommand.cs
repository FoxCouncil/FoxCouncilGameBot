// Copyright (c) 2018 The Fox Council

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FCGameBot.Models;
using Telegram.Bot.Types;

namespace FCGameBot.Commands
{
    internal class BankCommand : ICommand
    {
        private const string BankDefaultMemo = "Official Bank Transaction";

        #region Action Constants

        private const string ActionWithdraw = "withdraw";

        private const string ActionDeposit = "deposit";

        private const string ActionHome = "home";

        private const string ActionQuit = "quit";

        #endregion

        #region Inline Keyboards

        private readonly InlineKeyboard _homeKeyboard = new InlineKeyboard {
            {ActionWithdraw, "Withdraw"},
            {ActionDeposit, "Deposit"},
            {ActionQuit, "Cancel"}
        };

        private readonly InlineKeyboard _onlyCancelKeyboard = new InlineKeyboard {
            {ActionQuit, "Cancel"}
        };

        private readonly InlineKeyboard _onlyBackKeyboard = new InlineKeyboard {
            {ActionHome, "< Back"}
        };

        private readonly InlineKeyboard _withdrawKeyboard = new InlineKeyboard {
            {ActionHome, "< Back"}
        };

        private readonly InlineKeyboard _depositKeyboard = new InlineKeyboard {
            {ActionHome, "< Back"}
        };

        #endregion

        private const string HomeScreenTemplate = "Fox Bank: `You can trust us!`\n\n{0}: {1}";
        private const string HomeScreenNoWorldTemplate = HomeScreenTemplate + "\n\nYou are currently not in any worlds we serve. Thus you cannot deposit or withdraw.";

        public bool Private { get; } = true;

        public bool Public { get; } = true;

        public bool Admin { get; } = false;

        public bool Targetable { get; } = true;

        public string[] Names { get; } = { "bank" };

        public async Task Help(Queue<string> args, Player player)
        {
            await player.SendMessage("Nothing...");
        }

        public async Task Process(string alias, Queue<string> args, Status player, Status targetPlayer = null)
        {
            var bankAccount = GetAccount(player);

            var world = Game.GetSelectedWorld(player);

            if (world is null)
            {
                await this.SendInlineKeyboard(player, string.Format(HomeScreenNoWorldTemplate, bankAccount.Name, bankAccount.Balance.M()), _onlyCancelKeyboard);
            }
            else
            {
                await this.SendInlineKeyboard(player, string.Format(HomeScreenTemplate, bankAccount.Name, bankAccount.Balance.M()), _homeKeyboard);
            }
        }

        public async Task Callback(string data, Message msg, Player player)
        {
            var world = Game.GetSelectedWorld(player);

            if (world is null && data != ActionQuit)
            {
                var bankAccount = GetAccount(player);

                await msg.EditMessage(string.Format(HomeScreenNoWorldTemplate, bankAccount.Name, bankAccount.Balance.M()), _onlyBackKeyboard);
                return;
            }

            switch (data)
            {
                case ActionHome:
                {
                    var bankAccount = GetAccount(player);

                    await msg.EditMessage(string.Format(HomeScreenTemplate, bankAccount.Name, bankAccount.Balance.M()), _homeKeyboard);

                    return;
                }

                case ActionDeposit:
                {
                    var status = Game.GetStatus(player, world);

                    if (status.Credits <= 0)
                    {
                        await msg.EditMessage($"Insufficient funds in `{world?.Title}` world.", _onlyBackKeyboard);
                    }
                    else
                    {
                        await msg.EditMessage("How much would you like to deposit?", _depositKeyboard);
                    }

                    return;
                }

                case ActionWithdraw:
                {
                    var bankAccount = GetAccount(player);

                    if (bankAccount.Balance <= 0)
                    {
                        await msg.EditMessage($"Insufficient funds in the bank to withdraw.", _onlyBackKeyboard);
                    }
                    else
                    {
                        await msg.EditMessage("How much would you like to withdraw?", _withdrawKeyboard);
                    }

                    return;
                }

                default:
                {
                    await msg.Remove();
                    return;
                }
            }
        }

        private static void CreditAccount(BankAccount account, long amount, string memo = BankDefaultMemo)
        {
            amount = Math.Abs(amount);

            var newTransaction = new BankTransaction
            {
                AccountId = account.Id,
                Credit = amount,
                Memo = memo
            };

            Game.BankTransactions.Insert(newTransaction);

            account.Balance += amount;

            Game.BankAccounts.Update(account);            
        }

        private static bool DebitAccount(BankAccount account, long amount, string memo = BankDefaultMemo)
        {
            amount = Math.Abs(amount);

            if (account.Balance - amount < 0)
            {
                return false;
            }

            DebitAccountForced(account, amount, memo);

            return true;
        }

        private static void DebitAccountForced(BankAccount account, long amount, string memo = BankDefaultMemo)
        {
            amount = Math.Abs(amount);

            var newTransaction = new BankTransaction
            {
                AccountId = account.Id,
                Debit = amount,
                Memo = memo
            };

            Game.BankTransactions.Insert(newTransaction);

            account.Balance -= amount;

            Game.BankAccounts.Update(account);
        }

        private static BankAccount GetAccount(Status playerStatus)
        {
            return GetAccount(playerStatus.Player);
        }

        private static BankAccount GetAccount(Player bankPlayer)
        {
            var foundAccount = Game.BankAccounts.FindOne(x => x.OwnerId == bankPlayer.Id);

            if (foundAccount != null)
            {
                return foundAccount;
            }

            // Create The Default Account
            foundAccount = new BankAccount { Name = "Primary", Balance = 0, OwnerId = bankPlayer.Id };

            Game.BankAccounts.Insert(foundAccount);

            CreditAccount(foundAccount, 10000);

            bankPlayer.SendMessage($"Dear @{bankPlayer.Username},\n\nWe would like to personally welcome you as a new Fox Bank customer!\n\nAs a welcome gift, we have deposited {foundAccount.Balance.M()} into your personal account.\n\nDon't spend it all in once place; as we can't build an advertising profile of you.\n\nThank you,\n - Fox Bank Interdimensional Ltd.").Wait();

            return foundAccount;
        }
    }
}
