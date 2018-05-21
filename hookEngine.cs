///-----------------------------------------------------------------
///   Namespace:        kdrcts
///   Class:            HookEngine
///   Description:      Simple hook engine for c# projects
///   Author:           @kdrcetintas
///   Date:             2018-05-21
///   Version:          1.0
///   Notes:            Enjoy it.
///   Revision History:

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace kdrcts
{
    public static class HookEngine
    {

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public class HookEngineAppStartupMethod : Attribute
        {

        }

        public class HookOptions
        {
            public bool ThrowErrors { get; set; }
        }

        public class Hook
        {
            public int Priority { get; set; }
            public Action<object[]> Target { get; set; }
        }

        private static HookOptions Options;

        public static List<string> hookTypes = new List<string>();
        public static List<KeyValuePair<string, Hook>> hooks = new List<KeyValuePair<string, Hook>>();

        public static bool addHookType(string name)
        {
            try
            {
                if (!hookTypes.Contains(name))
                {
                    hookTypes.Add(name);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                if (Options.ThrowErrors)
                    throw e;
            }
            return false;
        }

        public static bool addHook(string list, Hook _Hook)
        {
            try
            {
                if (!hookTypes.Contains(list))
                    return false;

                if (hooks.Where(r => r.Key == list && r.Value.Target == _Hook.Target).Count() == 0)
                {
                    hooks.Add(new KeyValuePair<string, Hook>(list, _Hook));
                    return true;
                }
            }
            catch (Exception e)
            {
                if (Options.ThrowErrors)
                    throw e;
            }
            return false;
        }

        public static bool removeHook(string list, Hook _hook)
        {
            try
            {
                hooks.Where(r => r.Key == list && r.Value.Target == _hook.Target).Select(r => r).ToList().ForEach(b =>
                {
                    hooks.Remove(b);
                });
                return true;
            }
            catch (Exception e)
            {
                if (Options.ThrowErrors)
                    throw e;
            }
            return false;
        }

        public static List<Hook> getHooks(string list)
        {
            return hooks.Where(r => r.Key == list).OrderByDescending(r => r.Value.Priority).Select(r => r.Value).ToList();
        }

        public static void callHooks(string list, object[] _params, bool continueOnError = false)
        {
            try
            {
                hooks.Where(r => r.Key == list).OrderByDescending(r => r.Value.Priority).Select(r => r.Value).ToList().ForEach(r =>
                {
                    try
                    {
                        r.Target(_params);
                    }
                    catch (Exception e)
                    {
                        if (Options.ThrowErrors && continueOnError)
                            throw e;
                    }
                });
            }
            catch (Exception e)
            {
                if (Options.ThrowErrors)
                    throw e;
            }
        }

        public static void Start(HookOptions _Options, Assembly _MainAssembly)
        {
            Options = _Options;
            try
            {
                Assembly currentAssembly = _MainAssembly;
                var findedActions = currentAssembly.GetTypes()
                        .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static))
                        .Where(m =>
                        !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any()
                        &&
                        m.IsDefined(typeof(HookEngineAppStartupMethod), true)
                        )
                        .Select(x =>
                        new
                        {
                            Type = x.DeclaringType,
                            Class = x.DeclaringType.Name,
                            Action = x.Name
                        })
                        .OrderBy(x => x.Class).ThenBy(x => x.Action).ToList();

                foreach (var findedAction in findedActions)
                {
                    findedAction.Type.InvokeMember(findedAction.Action, BindingFlags.InvokeMethod | BindingFlags.Public |
                            BindingFlags.Static,
                        null,
                        null,
                        null);
                }
            }
            catch (Exception e)
            {
                if (Options.ThrowErrors)
                    throw e;
            }
        }
    }
}
