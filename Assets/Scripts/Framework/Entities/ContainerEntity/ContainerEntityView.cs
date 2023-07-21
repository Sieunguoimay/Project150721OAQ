using System;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtend.Attribute;
using Common.UnityExtend.Reflection;
using Framework.Resolver;
using Framework.Services.Data;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Entities.ContainerEntity
{
    public class ContainerEntityView<TEntity, TEntityData> : BaseEntityView<TEntity, TEntityData>
        where TEntity : class, IContainerEntity<IContainerEntityData, IContainerEntitySavedData>
        where TEntityData : IContainerEntityData
    {
        [SerializeField] private DataViewPair[] viewDataPairs;

        protected override void OnInject(IResolver resolver)
        {
            base.OnInject(resolver);
            foreach (var pair in viewDataPairs)
            {
                var data = Entity?.Components?.FirstOrDefault(c => c.Data.Id.Equals(pair.subId));
                if (data == null)
                {
                    Debug.Log("Component Entity is not found");
                    continue;
                }

                var view = pair.view as IManualView;
                view?.Setup(data);
            }
        }

        private void OnDestroy()
        {
            foreach (var pair in viewDataPairs)
            {
                (pair.view as IManualView)?.TearDown();
            }
        }
#if UNITY_EDITOR
        [ContextMenuExtend("TestSave")]
        public void TestSave()
        {
            Entity.SavedData.Save();
            foreach (var component in Entity.Components)
            {
                component.SavedData.Save();
            }
        }

        public IEnumerable<IEntityData> GetComponentDataItems() =>
            (DataAssetIdHelper.GetDataAsset<TEntity>(EntityId) as ContainerEntityData<TEntity>)?.GetComponentDataItems();

        public IEnumerable<string> GetComponentOptions() => GetComponentDataItems().Select(c => c.Id);
#endif
        [Serializable]
        public class DataViewPair
        {
            [TypeConstraint(false, typeof(IManualView))]
            public Object view;
#if UNITY_EDITOR
            [StringSelector(nameof(GetComponentOptions))]
#endif
            public string subId;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ContainerEntityView<,>.DataViewPair))]
    public class DataViewPairDrawer : PropertyDrawer
    {
        private bool _valid;
        private bool _shouldValidate;
        private SerializedProperty _view;
        private SerializedProperty _subId;

        public DataViewPairDrawer()
        {
            _shouldValidate = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _view = property.FindPropertyRelative("view");
            _subId = property.FindPropertyRelative("subId");

            if (_shouldValidate)
            {
                Validate(property);
                _shouldValidate = false;
            }

            EditorGUI.BeginChangeCheck();

            position.height -= 2;
            position.height /= 2;
            var color = GUI.color;
            if (!_valid)
            {
                GUI.color = Color.red;
            }

            EditorGUI.PropertyField(position, _view);

            GUI.color = color;
            position.y += 20;

            EditorGUI.PropertyField(position, _subId);

            if (EditorGUI.EndChangeCheck())
            {
                _shouldValidate = true;
            }
        }

        private void Validate(SerializedProperty property)
        {
            var entity = _view?.objectReferenceValue?.GetType().GetProperty("Entity", ReflectionUtility.PropertyFlags);
            var dataId = _subId.stringValue;
            var items = ReflectionUtility.GetDataFromMember(property?.serializedObject.targetObject,
                "GetComponentDataItems", false) as IEnumerable<IEntityData>;
            var dataType = items?.FirstOrDefault(i => i.Id.Equals(dataId))?.GetEntityType();
            _valid = entity?.PropertyType.IsAssignableFrom(dataType) ?? false;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2 + 2;
        }
    }
#endif
}