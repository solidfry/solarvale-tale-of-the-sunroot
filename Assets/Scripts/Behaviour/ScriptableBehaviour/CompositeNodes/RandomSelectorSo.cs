using System.Collections.Generic;
using System.Linq;
using Behaviour.ScriptableBehaviour.Base;
using Extensions;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour.CompositeNodes
{
    [CreateAssetMenu(fileName = "New Random Selector", menuName = "Behaviours/Composite/Random Selector", order = 1)]
    public class RandomSelectorSo : PrioritySelectorSo
    {
        protected override List<NodeSo> Sort() => Children.Shuffle().ToList();
    }
}