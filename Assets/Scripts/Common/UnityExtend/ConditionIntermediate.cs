using System;
using System.Linq;
using Common.UnityExtend.Attribute;
using UnityEngine;
using UnityEngine.Events;

namespace Common.UnityExtend
{
    public class ConditionIntermediate : MonoBehaviour
    {
        [SerializeField] private IfStatement ifStatement;
        [SerializeField] private UnityEvent then;

        [Serializable]
        private class IfStatement
        {
            public Field fieldA;
            public CompareOperator compareOperator;
            public Field fieldB;

            public bool IsTrue()
            {
                var valueA = fieldA.GetValue();
                var valueB = fieldB.GetValue();
                var typeA = valueA.GetType();
                var typeB = valueB.GetType();
                if (typeA == typeB)
                {
                    return CompareTwoFieldOfSameType(valueA, valueB, typeA, compareOperator);
                }

                Debug.LogError("Comparing between two different type");
                return false;
            }

            private static bool CompareTwoFieldOfSameType(object fieldA, object fieldB, Type type, CompareOperator compareOperator)
            {
                if (compareOperator == CompareOperator.Equal)
                {
                    return fieldA.Equals(fieldB);
                }

                return type == typeof(double) && CompareTwoDouble((double) fieldA, (double) fieldB, compareOperator);
            }

            private static bool CompareTwoDouble(double a, double b, CompareOperator compareOperator)
            {
                return compareOperator switch
                {
                    CompareOperator.Equal => Math.Abs(a - b) <= 0,
                    CompareOperator.NotEqual => Math.Abs(a - b) > 0,
                    CompareOperator.GreaterThan => a > b,
                    CompareOperator.LessThan => a < b,
                    CompareOperator.GreaterThanOrEqual => a >= b,
                    CompareOperator.LessThanOrEqual => a <= b,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        private enum CompareOperator
        {
            Equal,
            NotEqual,
            LessThan,
            GreaterThan,
            LessThanOrEqual,
            GreaterThanOrEqual,
        }

        private enum FieldType
        {
            Bool,
            Text,
            Number,
            ObjectField,
        }

        [Serializable]
        private class Field
        {
            public FieldType type;

            [ShowIf(nameof(type), FieldType.ObjectField), SerializeField]
            private UnityEngine.Object sourceObject;

            [ShowIf(nameof(type), FieldType.ObjectField), SerializeField] [StringSelector(nameof(ObjectFields))]
            private string path;

            [ShowIf(nameof(type), FieldType.Text), SerializeField]
            private string stringValue;

            [ShowIf(nameof(type), FieldType.Number), SerializeField]
            private double numberValue;

            [ShowIf(nameof(type), FieldType.Bool), SerializeField]
            private bool boolValue;

            public string[] ObjectFields
            {
                get { return sourceObject.GetType().GetProperties().Select(p => p.Name).ToArray(); }
            }

            public object GetValue()
            {
                return type switch
                {
                    FieldType.Bool => boolValue,
                    FieldType.Text => stringValue,
                    FieldType.Number => numberValue,
                    FieldType.ObjectField => ReflectionUtility.GetPropertyOrFieldValue(sourceObject, path),
                    _ => null
                };
            }
        }

        [ContextMenu("Trigger")]
        public void Trigger()
        {
            if (ifStatement.IsTrue())
            {
                then?.Invoke();
            }
        }

        [ContextMenu("Test")]
        private void Test()
        {
            Debug.Log(ifStatement.IsTrue());
        }
    }
}