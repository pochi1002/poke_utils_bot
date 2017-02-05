using PokeUtils.Models.Functions;
using PokeUtils.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PokeUtils.Models
{
    class ContextManager
    {
        ConcurrentDictionary<string, IFunctionProvider> Contexts { get; } = new ConcurrentDictionary<string, IFunctionProvider>();

        public IFunctionProvider GetContextOf(string name)
        {
            return this.Contexts.ForceGetValue(name, () => new MetamonExpFunctionProvider(name));
        }

        internal void Forget(string id)
        {
            var function = default(IFunctionProvider);
            this.Contexts.TryRemove(id, out function);
        }

        internal void ForgetAll()
        {
            this.Contexts.Clear();
        }
    }
}