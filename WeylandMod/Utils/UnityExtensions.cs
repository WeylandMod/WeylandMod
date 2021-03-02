using System;
using System.Reflection;
using UnityEngine;

namespace WeylandMod.Utils
{
    internal static class UnityExtensions
    {
        public static T AddComponentCopy<T>(this GameObject self, T other) where T : Component
        {
            return self.AddComponent<T>().CopyComponent(other);
        }

        private static T CopyComponent<T>(this Component self, T other) where T : Component
        {
            // https://answers.unity.com/questions/530178/how-to-get-a-component-from-an-object-and-add-it-t.html

            var type = other.GetType();
            if (self.GetType() != type)
            {
                throw new InvalidOperationException(
                    $"Failed to copy component of type {type} into type {self.GetType()}."
                );
            }

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (var field in type.GetFields(flags))
            {
                field.SetValue(self, field.GetValue(other));
            }

            foreach (var property in type.GetProperties(flags))
            {
                property.SetValue(self, property.GetValue(other));
            }

            return self as T;
        }
    }
}