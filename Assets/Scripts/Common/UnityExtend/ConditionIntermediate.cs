using System;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtend.Attribute;
using Common.UnityExtend.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

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
                return CompareTwoFieldOfSameType(valueA, valueB, fieldA.GetResultType(), compareOperator);
            }

            private static bool CompareTwoFieldOfSameType(object fieldA, object fieldB, FieldDataSourceType type, CompareOperator compareOperator)
            {
                if (compareOperator == CompareOperator.Equal)
                {
                    return fieldA.Equals(fieldB);
                }

                return type == FieldDataSourceType.Number && CompareTwoDouble((double)fieldA, (double) fieldB, compareOperator);
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

        private enum FieldDataSourceType
        {
            Bool,
            Text,
            Number,
            ObjectField,
        }

        [Serializable]
        private class Field
        {
            public FieldDataSourceType dataSource;

            [ShowIf(nameof(dataSource), FieldDataSourceType.ObjectField), SerializeField, UnityObjectSelector]
            private UnityEngine.Object sourceObject;

            [ShowIf(nameof(dataSource), FieldDataSourceType.ObjectField), SerializeField] [StringSelector(nameof(ObjectFields))]
            private string path;

            [ShowIf(nameof(dataSource), FieldDataSourceType.Text), SerializeField]
            private string stringValue;

            [ShowIf(nameof(dataSource), FieldDataSourceType.Number), SerializeField]
            private double numberValue;

            [ShowIf(nameof(dataSource), FieldDataSourceType.Bool), SerializeField]
            private bool boolValue;

            public IEnumerable<string> ObjectFields
            {
                get { return sourceObject.GetType().GetProperties().Select(p => p.Name); }
            }

            public object GetValue()
            {
                return dataSource switch
                {
                    FieldDataSourceType.Bool => boolValue,
                    FieldDataSourceType.Text => stringValue,
                    FieldDataSourceType.Number => numberValue,
                    FieldDataSourceType.ObjectField => ReflectionUtility.GetPropertyOrFieldValue(sourceObject, path),
                    _ => null
                };
            }

            public FieldDataSourceType GetResultType()
            {
                if (dataSource == FieldDataSourceType.ObjectField)
                {
                    return MapToTypeEnum(ReflectionUtility.GetPropertyOrFieldType(sourceObject.GetType(), path));
                }

                return dataSource;
            }

            public static FieldDataSourceType MapToTypeEnum(Type type)
            {
                if (type == typeof(bool))
                {
                    return FieldDataSourceType.Bool;
                }

                if (type == typeof(double) || type == typeof(float) || type == typeof(int))
                {
                    return FieldDataSourceType.Number;
                }

                if (type == typeof(string))
                {
                    return FieldDataSourceType.Text;
                }

                return FieldDataSourceType.ObjectField;
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

        public void OnEventA()
        {
            Debug.Log("OnEventA");
        }
        public void OnEventB(object sender, EventArgs args)
        {
            Debug.Log("OnEventB");
        }
    }
}