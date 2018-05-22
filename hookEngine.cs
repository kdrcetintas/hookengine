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

namespace kdrcts.kHelpers
{
    public static class HookEngine
    {

        /// <summary>
        /// When HookEngine.Start() void is called, any void on your project with this attribute will called via reflection.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public class HookEngineAppStartupMethod : Attribute
        {}

        public class HookOptions
        {
            public bool ThrowErrors { get; set; }
        }

        public class Hook
        {
            /// <summary>
            /// Hook registrations will called with ordering by Priority DESC
            /// </summary>
            public int Priority { get; set; }
            /// <summary>
            /// Give a method address for callback
            /// </summary>
            public Action<object[]> Target { get; set; }
        }

        private static HookOptions Options;

        /// <summary>
        /// Container for hookTypes
        /// </summary>
        private static List<string> hookTypes = new List<string>();

        /// <summary>
        /// Container for hooks
        /// </summary>
        private static List<KeyValuePair<string, Hook>> hooks = new List<KeyValuePair<string, Hook>>();

        /// <summary>
        /// Create hook types for seperating and call with specific hook registrations.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Create hook registrations for specific type list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="_Hook"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Remove a hook registration from specific type list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="_hook"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get all hook registrations from specific type list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<Hook> getHooks(string list)
        {
            return hooks.Where(r => r.Key == list).OrderByDescending(r => r.Value.Priority).Select(r => r.Value).ToList();
        }

        /// <summary>
        /// Get all hook types
        /// </summary>
        /// <returns></returns>
        public static List<string> getHookTypes()
        {
            return hookTypes.ToList();
        }

        /// <summary>
        /// Call all hook registrations for specific registrations
        /// </summary>
        /// <param name="list"></param>
        /// <param name="_params"></param>
        /// <param name="continueOnError"></param>
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

        /// <summary>
        /// Start the engine at your project startup.
        /// It's could be OwinStartup class or Global.asax or WebApiConfig.cs for web projects
        /// or
        /// Could be Program.cs for Desktop projects
        /// </summary>
        /// <param name="_Options"></param>
        /// <param name="_MainAssembly"></param>
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
