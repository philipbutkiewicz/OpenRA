#region Copyright & License Information
/*
 * Copyright 2007-2014 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.RA.Render;
using OpenRA.Support;
using OpenRA.Traits;

namespace OpenRA.Mods.RA
{
	class SeedsResourceInfo : TraitInfo<SeedsResource>
	{
		public readonly int Interval = 75;
		public readonly string ResourceType = "Ore";
		public readonly int MaxRange = 100;
		public readonly int AnimationInterval = 750;
	}

	class SeedsResource : ITick, ISeedableResource
	{
		int ticks;
		int animationTicks;

		public void Tick(Actor self)
		{
			if (--ticks <= 0)
			{
				Seed(self);
				ticks = self.Info.Traits.Get<SeedsResourceInfo>().Interval;
			}

			if (--animationTicks <= 0)
			{
				var info = self.Info.Traits.Get<SeedsResourceInfo>();
				self.Trait<RenderBuilding>().PlayCustomAnim(self, "active");
				animationTicks = info.AnimationInterval;
			}
		}

		public void Seed(Actor self)
		{
			var info = self.Info.Traits.Get<SeedsResourceInfo>();
			var resourceType = self.World.WorldActor
				.TraitsImplementing<ResourceType>()
				.FirstOrDefault(t => t.Info.Name == info.ResourceType);

			if (resourceType == null)
				throw new InvalidOperationException("No such resource type `{0}`".F(info.ResourceType));

			var resLayer = self.World.WorldActor.Trait<ResourceLayer>();

			var cell = RandomWalk(self.Location, self.World.SharedRandom)
				.Take(info.MaxRange)
				.SkipWhile(p => resLayer.GetResource(p) == resourceType && resLayer.IsFull(p))
				.Cast<CPos?>().FirstOrDefault();

			if (cell != null && resLayer.CanSpawnResourceAt(resourceType, cell.Value))
				resLayer.AddResource(resourceType, cell.Value, 1);
		}

		static IEnumerable<CPos> RandomWalk(CPos p, MersenneTwister r)
		{
			for (;;)
			{
				var dx = r.Next(-1, 2);
				var dy = r.Next(-1, 2);

				if (dx == 0 && dy == 0)
					continue;

				p += new CVec(dx, dy);
				yield return p;
			}
		}
	}
}
