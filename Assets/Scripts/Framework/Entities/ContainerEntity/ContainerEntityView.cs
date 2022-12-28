using System;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtend.Attribute;
using Framework.Entities;
using Framework.Resolver;
using Framework.Services.Data;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Entities.ContainerEntity
{
    public class ContainerEntityView<TEntity, TData> : BaseEntityView<TEntity, TData>
        where TEntity : class, IContainerEntity<IContainerEntityData, IContainerEntitySavedData> where TData : IContainerEntityData
    {
        [SerializeField] private DataViewPair[] pairs;

        public override void Inject(IResolver resolver)
        {
            base.Inject(resolver);
        }

        protected override void SetupInternal()
        {
            base.SetupInternal();
        }
#if UNITY_EDITOR
        [ContextMenu("TestSave")]
        public void TestSave()
        {
            Entity.SavedData.Save();
            foreach (var component in Entity.Components)
            {
                component.SavedData.Save();
            }
        }

        public IEnumerable<IEntityData> GetComponentDataItems() => (IdsHelper.GetDataAsset<TEntity>(EntityId) as ContainerEntityData<TEntity>)?.GetComponentDataItems();
        public IEnumerable<string> GetComponentOptions() => GetComponentDataItems().Select(c => c.Id);
#endif
        [Serializable]
        public class DataViewPair
        {
            [ComponentSelector] public Object view;
#if UNITY_EDITOR
            [ObjectSelector(nameof(GetComponentDataItems), true)]
#endif
            public Object data;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ContainerEntityView<,>.DataViewPair))]
    public class DataViewPairDrawer : PropertyDrawer
    {
        private bool _valid;
        private bool _shouldValidate;

        public DataViewPairDrawer()
        {
            _shouldValidate = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
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

            var view = property.FindPropertyRelative("view");
            var data = property.FindPropertyRelative("data");
            EditorGUI.PropertyField(position, view, new GUIContent("view"));

            GUI.color = color;
            position.y += 20;

            EditorGUI.PropertyField(position, data, new GUIContent("data"));

            if (EditorGUI.EndChangeCheck()) 
            {
                _shouldValidate = true;
            }
        }

        private void Validate(SerializedProperty property)
        {
            var entity = property.FindPropertyRelative("view").objectReferenceValue?.GetType().GetProperty("Entity");
            var dataType = (property.FindPropertyRelative("data").objectReferenceValue as IEntityData)?.GetEntityType();
            _valid = entity?.PropertyType.IsAssignableFrom(dataType) ?? false;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2 + 2;
        }
    }
#endif
}