#region Copyright & License Information
/*
 * Copyright 2007-2019 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Diagnostics;
using OpenRA.Support;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.Logic
{
	public class PerfDebugLogic : ChromeLogic
	{
		[ObjectCreator.UseCtor]
		public PerfDebugLogic(Widget widget)
		{
			var perfGraph = widget.Get("GRAPH_BG");
			perfGraph.IsVisible = () => Game.Settings.Debug.PerfGraph;

			var perfText = widget.Get<LabelWidget>("PERF_TEXT");
			perfText.IsVisible = () => Game.Settings.Debug.PerfText;

			var fpsTimer = Stopwatch.StartNew();
			var fpsReferenceFrame = 0;
			var fps = 0;
			Dictionary<string, object> perf = new Dictionary<string, object>();
			perfText.GetText = () =>
			{
				var elapsed = fpsTimer.ElapsedMilliseconds;
				if (elapsed > 1000)
				{
					// Round to closest integer
					fps = (int)(1000.0f * (Game.RenderFrame - fpsReferenceFrame) / fpsTimer.ElapsedMilliseconds + 0.5f);
					fpsTimer.Restart();
					fpsReferenceFrame = Game.RenderFrame;
				}

				perf["fps"] = fps;
				perf["tik"] = Game.LocalTick;
				perf["tik_time"] = "{0:F1}".F(PerfHistory.Items["tick_time"].Average(Game.Settings.Debug.Samples));
				perf["renderFrame"] = Game.RenderFrame;
				perf["render"] = "{0:F1}".F(PerfHistory.Items["render"].Average(Game.Settings.Debug.Samples));
				perf["batches"] = PerfHistory.Items["batches"].LastValue;

				return FieldLoader.TranslateUI("PERFDEBUG-TEXT", perf);
			};
		}
	}
}
