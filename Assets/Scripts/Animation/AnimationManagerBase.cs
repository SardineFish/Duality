using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Duality
{
    /// <summary>
    /// This helper base class extract all AnimationClip, IEnumerable<AnimationClip> and IDictionary<TKey, AnimationClip> from derived class
    /// Allow editing all thess animation clips in animation editor
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public abstract class AnimationManagerBase : MonoBehaviour, IAnimationClipSource
    {
        public virtual void GetAnimationClips(List<AnimationClip> results)
        {
            GetAnimationClipMember(this, results);
        }

        public static void GetAnimationClipMember<T>(T component, List<AnimationClip> results)
        {
            var animFields = component.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(fieldInfo => fieldInfo.FieldType == typeof(AnimationClip))
                .Select(fieldInfo => fieldInfo.GetValue(component) as AnimationClip);
            results.AddRange(animFields);

            var animLists =  component.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(fieldinfo => (typeof(IEnumerable<AnimationClip>).IsAssignableFrom(fieldinfo.FieldType)))
                .SelectMany(fieldInfo => fieldInfo.GetValue(component) as IEnumerable<AnimationClip>);
            results.AddRange(animLists);

            var animDict =  component.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(fieldInfo => typeof(IDictionary).IsAssignableFrom(fieldInfo.FieldType))
                .Select(fieldInfo => (fieldInfo,
                    fieldInfo.FieldType.GetProperty("Values", BindingFlags.Instance | BindingFlags.Public)))
                .Where((fieldInfo) => typeof(IEnumerable<AnimationClip>).IsAssignableFrom(fieldInfo.Item2.PropertyType))
                .SelectMany((fieldInfo) =>
                    fieldInfo.Item2.GetValue(fieldInfo.fieldInfo.GetValue(component)) as IEnumerable<AnimationClip>);
            results.AddRange(animDict);


            var animatorFields = component.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(fieldInfo => fieldInfo.FieldType == typeof(RuntimeAnimatorController))
                .Select(fieldInfo => fieldInfo.GetValue(component) as RuntimeAnimatorController)
                .SelectMany(animator => animator.animationClips);
            results.AddRange(animatorFields);
        }
        
    }
}