using UnityEngine;

namespace Common.Transaction
{
    public interface ITransaction
    {
        void Send();
        void Confirm();
        void Cancel();
    }

    public class BaseTransaction : ITransaction
    {
        private bool _activeState;
        private readonly ITransactionResult _transactionResult;

        public BaseTransaction(ITransactionResult transactionResult)
        {
            _transactionResult = transactionResult;
        }

        public void Send()
        {
            if (!_activeState)
            {
                //Sent!
                _activeState = true;
                _transactionResult?.OnSend();
            }
            else
            {
                Debug.LogError("Transaction has already been sent");
            }        }

        public void Confirm()
        {
            if (_activeState)
            {
                _activeState = false;
                _transactionResult?.OnSuccess();
            }
            else
            {
                Debug.LogError("Transaction has already been confirmed or cancelled");
            }
        }

        public void Cancel()
        {
            if (_activeState)
            {
                _activeState = false;
                _transactionResult?.OnDiscard();
            }
            else
            {
                Debug.LogError("Transaction has already been confirmed or cancelled");
            }
        }
    }

    public interface ITransactionResult
    {
        void OnSend();
        void OnSuccess();
        void OnDiscard();
    }
}