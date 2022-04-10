using System.Collections.Generic;
using Civ2engine.Advances;
using Civ2engine.Enums;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Civ2engine.Scripting
{
    public class Tech
    {
        private readonly IList<Advance> _advances;
        private readonly Advance _advance;
        
        public Tech(IList<Advance> advances, int index)
        {
            _advances = advances;
            _advance = _advances[index];
        }

        /// <summary>
        /// Returns the AI value of the tech.
        /// </summary>
        public int aiValue
        {
            get => _advance.AIvalue;
            set => _advance.AIvalue = value;
        }


        /// <summary>
        /// Returns the category of the tech.
        /// </summary>
        public int category
        {
            get => (int)_advance.KnowledgeCategory;
            set => _advance.KnowledgeCategory = (KnowledgeType)value;
        } 

        /// <summary>
        ///Returns the epoch of the tech.
        /// </summary>
        public int epoch
        {
            get => (int)_advance.Epoch;
            set => _advance.Epoch = (EpochType)value;
        }

        /// <summary>
        /// Returns the group of the tech.
        /// </summary>
        public int group
        {
            get => _advance.AdvanceGroup;
            set => _advance.AdvanceGroup = value;
        }

        /// <summary>
        /// Returns the id of the tech.
        /// </summary>
        public int id => _advance.Index;

        /// <summary>
        /// Returns the modifier to the AI value based on leader personality.
        /// </summary>
        public int modifier
        {
            get => _advance.Modifier;
            set => _advance.Modifier = value;
        }

        /// <summary>
        /// Returns the name of the tech.
        /// </summary>
        public string name
        {
            get => _advance.Name;
            set => _advance.Name = value;
        }

        /// <summary>
        /// Returns the first prerequisite of the tech.
        /// </summary>
        public Tech prereq1
        {
            get
            {
                if (_advance.Prereq1 != AdvancesConstants.No && _advance.Prereq2 != AdvancesConstants.Nil)
                {
                    return new Tech(_advances, _advance.Prereq1);
                }
                return null;
            }
            set => _advance.Prereq1 = value?.id ?? AdvancesConstants.Nil;
        }
        
        /// <summary>
        /// Returns the second prerequisite of the tech.
        /// </summary>
        public Tech prereq2
        {
            get
            {
                if (_advance.Prereq2 != AdvancesConstants.No && _advance.Prereq2 != AdvancesConstants.Nil)
                {
                    return new Tech(_advances, _advance.Prereq1);
                }
                return null;
            }
            set => _advance.Prereq2 = value?.id ?? AdvancesConstants.Nil;
        }


        /// <summary>
        /// Returns whether or not any tribe has researched the tech.
        /// </summary>
        public bool researched => AdvanceFunctions.HasAdvanceBeenDiscovered(_advance.Index);


        public IList<ConstructionAbility> AllowBuilding => _advance.ImprovementsEnabled;
    }
}