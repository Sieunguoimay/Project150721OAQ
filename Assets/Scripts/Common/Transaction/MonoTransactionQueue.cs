using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common.Transaction
{
    public class MonoTransactionQueue : MonoBehaviour, ITransactionQueue
    {
        private ITransactionQueue _transactionQueue;
        private void Start()
        {
            _transactionQueue = new BaseTransactionQueue();
        }

        private void OnDestroy()
        {
            CancelAll();
        }

        private void OnApplicationQuit()
        {
            CancelAll();
        }

        public void Send(ITransaction transaction) => _transactionQueue.Send(transaction);
        public void Send(ITransaction[] transactions) => _transactionQueue.Send(transactions);
        public void TakeOutTransaction() => _transactionQueue.TakeOutTransaction();
        public void TakeOutTransaction(int num) => _transactionQueue.TakeOutTransaction(num);
        public void CancelAll() => _transactionQueue.CancelAll();
        public Queue<ITransaction> Queue => _transactionQueue.Queue;
    }
}