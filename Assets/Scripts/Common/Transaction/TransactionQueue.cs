using System.Collections.Generic;
using UnityEngine;

namespace Common.Transaction
{
    public interface ITransactionQueue
    {
        void Send(ITransaction transaction);
        void Send(ITransaction[] transactions);
        void TakeOutTransaction();
        void TakeOutTransaction(int num);
        void CancelAll();
        Queue<ITransaction> Queue { get; }
    }

    public class BaseTransactionQueue : ITransactionQueue
    {
        public Queue<ITransaction> Queue { get; } = new();

        public void Send(ITransaction transaction)
        {
            transaction.Send();
            Queue.Enqueue(transaction);
        }

        public void Send(ITransaction[] transactions)
        {
            for (var i = 0; i < transactions.Length; i++)
            {
                var transaction = transactions[i];
                transaction.Send();
                Queue.Enqueue(transaction);
            }
        }

        public void TakeOutTransaction()
        {
            Queue.Dequeue().Confirm();
        }

        public void TakeOutTransaction(int num)
        {
            var n = Mathf.Min(num, Queue.Count);
            for (var i = 0; i < n; i++)
            {
                TakeOutTransaction();
            }
        }

        public void CancelAll()
        {
            var n = Queue.Count;
            for (var i = 0; i < n; i++)
            {
                Queue.Dequeue().Cancel();
            }
        }
    }
}