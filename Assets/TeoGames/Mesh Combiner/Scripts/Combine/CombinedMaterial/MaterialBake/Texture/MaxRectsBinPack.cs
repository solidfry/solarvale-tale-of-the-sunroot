using System;
using System.Collections.Generic;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.CombinedMaterial.MaterialBake.Texture {
	public enum FreeRectChoiceHeuristic {
		RectBestShortSideFit,
		RectBestLongSideFit,
		RectBestAreaFit,
		RectBottomLeftRule,
		RectContactPointRule
	};

	[Serializable]
	public class MaxRectsBinPack {
		public int binWidth = 0;
		public int binHeight = 0;
		public bool allowRotations;

		public List<Rect> usedRectangles = new List<Rect>();
		public List<Rect> freeRectangles = new List<Rect>();

		public MaxRectsBinPack(int width, int height, bool rotations = true) {
			Init(width, height, rotations);
		}

		public void Init(int width, int height, bool rotations = true) {
			binWidth = width;
			binHeight = height;
			allowRotations = rotations;

			var n = new Rect {
				x = 0,
				y = 0,
				width = width,
				height = height
			};

			usedRectangles.Clear();
			freeRectangles.Clear();
			freeRectangles.Add(n);
		}

		public Rect Insert(int width, int height, FreeRectChoiceHeuristic method) {
			var newNode = new Rect();
			var score1 = 0;
			var score2 = 0;
			newNode = method switch {
				FreeRectChoiceHeuristic.RectBestShortSideFit => FindPositionForNewNodeBestShortSideFit(
					width, height, ref score1, ref score2
				),
				FreeRectChoiceHeuristic.RectBottomLeftRule => FindPositionForNewNodeBottomLeft(
					width, height, ref score1, ref score2
				),
				FreeRectChoiceHeuristic.RectContactPointRule => FindPositionForNewNodeContactPoint(
					width, height, ref score1
				),
				FreeRectChoiceHeuristic.RectBestLongSideFit => FindPositionForNewNodeBestLongSideFit(
					width, height, ref score2, ref score1
				),
				FreeRectChoiceHeuristic.RectBestAreaFit => FindPositionForNewNodeBestAreaFit(
					width, height, ref score1, ref score2
				),
				_ => newNode
			};

			if (newNode.height == 0) return newNode;

			var numRectanglesToProcess = freeRectangles.Count;
			for (var i = 0; i < numRectanglesToProcess; ++i) {
				if (SplitFreeNode(freeRectangles[i], ref newNode)) {
					freeRectangles.RemoveAt(i);
					--i;
					--numRectanglesToProcess;
				}
			}

			PruneFreeList();

			usedRectangles.Add(newNode);
			return newNode;
		}

		public void Insert(List<Rect> rects, List<Rect> dst, FreeRectChoiceHeuristic method) {
			dst.Clear();

			while (rects.Count > 0) {
				var bestScore1 = int.MaxValue;
				var bestScore2 = int.MaxValue;
				var bestRectIndex = -1;
				var bestNode = new Rect();

				for (var i = 0; i < rects.Count; ++i) {
					var score1 = 0;
					var score2 = 0;
					var newNode = ScoreRect((int)rects[i].width, (int)rects[i].height, method, ref score1, ref score2);

					if (score1 < bestScore1 || (score1 == bestScore1 && score2 < bestScore2)) {
						bestScore1 = score1;
						bestScore2 = score2;
						bestNode = newNode;
						bestRectIndex = i;
					}
				}

				if (bestRectIndex == -1)
					return;

				PlaceRect(bestNode);
				rects.RemoveAt(bestRectIndex);
			}
		}

		void PlaceRect(Rect node) {
			var numRectanglesToProcess = freeRectangles.Count;
			for (var i = 0; i < numRectanglesToProcess; ++i) {
				if (SplitFreeNode(freeRectangles[i], ref node)) {
					freeRectangles.RemoveAt(i);
					--i;
					--numRectanglesToProcess;
				}
			}

			PruneFreeList();

			usedRectangles.Add(node);
		}

		Rect ScoreRect(int width, int height, FreeRectChoiceHeuristic method, ref int score1, ref int score2) {
			var newNode = new Rect();
			score1 = int.MaxValue;
			score2 = int.MaxValue;
			switch (method) {
				case FreeRectChoiceHeuristic.RectBestShortSideFit:
					newNode = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2);
					break;
				case FreeRectChoiceHeuristic.RectBottomLeftRule:
					newNode = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2);
					break;
				case FreeRectChoiceHeuristic.RectContactPointRule:
					newNode = FindPositionForNewNodeContactPoint(width, height, ref score1);
					score1 = -score1;
					break;
				case FreeRectChoiceHeuristic.RectBestLongSideFit:
					newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1);
					break;
				case FreeRectChoiceHeuristic.RectBestAreaFit:
					newNode = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2);
					break;
			}

			if (newNode.height == 0) {
				score1 = int.MaxValue;
				score2 = int.MaxValue;
			}

			return newNode;
		}

		public float Occupancy() {
			ulong usedSurfaceArea = 0;
			for (var i = 0; i < usedRectangles.Count; ++i) {
				usedSurfaceArea += (uint)usedRectangles[i].width * (uint)usedRectangles[i].height;
			}

			return (float)usedSurfaceArea / (binWidth * binHeight);
		}

		Rect FindPositionForNewNodeBottomLeft(int width, int height, ref int bestY, ref int bestX) {
			var bestNode = new Rect();

			bestY = int.MaxValue;

			for (var i = 0; i < freeRectangles.Count; ++i) {
				if (freeRectangles[i].width >= width && freeRectangles[i].height >= height) {
					var topSideY = (int)freeRectangles[i].y + height;
					if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].x < bestX)) {
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = width;
						bestNode.height = height;
						bestY = topSideY;
						bestX = (int)freeRectangles[i].x;
					}
				}

				if (allowRotations && freeRectangles[i].width >= height && freeRectangles[i].height >= width) {
					var topSideY = (int)freeRectangles[i].y + width;
					if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].x < bestX)) {
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = height;
						bestNode.height = width;
						bestY = topSideY;
						bestX = (int)freeRectangles[i].x;
					}
				}
			}

			return bestNode;
		}

		Rect FindPositionForNewNodeBestShortSideFit(int width, int height, ref int bestShortSideFit,
			ref int bestLongSideFit) {
			var bestNode = new Rect();

			bestShortSideFit = int.MaxValue;

			for (var i = 0; i < freeRectangles.Count; ++i) {
				if (freeRectangles[i].width >= width && freeRectangles[i].height >= height) {
					var leftoverHoriz = Mathf.Abs((int)freeRectangles[i].width - width);
					var leftoverVert = Mathf.Abs((int)freeRectangles[i].height - height);
					var shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);
					var longSideFit = Mathf.Max(leftoverHoriz, leftoverVert);

					if (shortSideFit < bestShortSideFit ||
					    (shortSideFit == bestShortSideFit && longSideFit < bestLongSideFit)) {
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = width;
						bestNode.height = height;
						bestShortSideFit = shortSideFit;
						bestLongSideFit = longSideFit;
					}
				}

				if (allowRotations && freeRectangles[i].width >= height && freeRectangles[i].height >= width) {
					var flippedLeftoverHoriz = Mathf.Abs((int)freeRectangles[i].width - height);
					var flippedLeftoverVert = Mathf.Abs((int)freeRectangles[i].height - width);
					var flippedShortSideFit = Mathf.Min(flippedLeftoverHoriz, flippedLeftoverVert);
					var flippedLongSideFit = Mathf.Max(flippedLeftoverHoriz, flippedLeftoverVert);

					if (flippedShortSideFit < bestShortSideFit || (flippedShortSideFit == bestShortSideFit &&
					                                               flippedLongSideFit < bestLongSideFit)) {
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = height;
						bestNode.height = width;
						bestShortSideFit = flippedShortSideFit;
						bestLongSideFit = flippedLongSideFit;
					}
				}
			}

			return bestNode;
		}

		Rect FindPositionForNewNodeBestLongSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit) {
			var bestNode = new Rect();

			bestLongSideFit = int.MaxValue;

			for (var i = 0; i < freeRectangles.Count; ++i) {
				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRectangles[i].width >= width && freeRectangles[i].height >= height) {
					var leftoverHoriz = Mathf.Abs((int)freeRectangles[i].width - width);
					var leftoverVert = Mathf.Abs((int)freeRectangles[i].height - height);
					var shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);
					var longSideFit = Mathf.Max(leftoverHoriz, leftoverVert);

					if (longSideFit < bestLongSideFit ||
					    (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit)) {
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = width;
						bestNode.height = height;
						bestShortSideFit = shortSideFit;
						bestLongSideFit = longSideFit;
					}
				}

				if (allowRotations && freeRectangles[i].width >= height && freeRectangles[i].height >= width) {
					var leftoverHoriz = Mathf.Abs((int)freeRectangles[i].width - height);
					var leftoverVert = Mathf.Abs((int)freeRectangles[i].height - width);
					var shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);
					var longSideFit = Mathf.Max(leftoverHoriz, leftoverVert);

					if (longSideFit < bestLongSideFit ||
					    (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit)) {
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = height;
						bestNode.height = width;
						bestShortSideFit = shortSideFit;
						bestLongSideFit = longSideFit;
					}
				}
			}

			return bestNode;
		}

		Rect FindPositionForNewNodeBestAreaFit(int width, int height, ref int bestAreaFit, ref int bestShortSideFit) {
			var bestNode = new Rect();

			bestAreaFit = int.MaxValue;

			for (var i = 0; i < freeRectangles.Count; ++i) {
				var areaFit = (int)freeRectangles[i].width * (int)freeRectangles[i].height - width * height;

				if (freeRectangles[i].width >= width && freeRectangles[i].height >= height) {
					var leftoverHoriz = Mathf.Abs((int)freeRectangles[i].width - width);
					var leftoverVert = Mathf.Abs((int)freeRectangles[i].height - height);
					var shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);

					if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit)) {
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = width;
						bestNode.height = height;
						bestShortSideFit = shortSideFit;
						bestAreaFit = areaFit;
					}
				}

				if (allowRotations && freeRectangles[i].width >= height && freeRectangles[i].height >= width) {
					var leftoverHoriz = Mathf.Abs((int)freeRectangles[i].width - height);
					var leftoverVert = Mathf.Abs((int)freeRectangles[i].height - width);
					var shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);

					if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit)) {
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = height;
						bestNode.height = width;
						bestShortSideFit = shortSideFit;
						bestAreaFit = areaFit;
					}
				}
			}

			return bestNode;
		}

		int CommonIntervalLength(int i1Start, int i1End, int i2Start, int i2End) {
			if (i1End < i2Start || i2End < i1Start)
				return 0;
			return Mathf.Min(i1End, i2End) - Mathf.Max(i1Start, i2Start);
		}

		int ContactPointScoreNode(int x, int y, int width, int height) {
			var score = 0;

			if (x == 0 || x + width == binWidth)
				score += height;
			if (y == 0 || y + height == binHeight)
				score += width;

			for (var i = 0; i < usedRectangles.Count; ++i) {
				if (usedRectangles[i].x == x + width || usedRectangles[i].x + usedRectangles[i].width == x)
					score += CommonIntervalLength(
						(int)usedRectangles[i].y, (int)usedRectangles[i].y + (int)usedRectangles[i].height, y,
						y + height
					);
				if (usedRectangles[i].y == y + height || usedRectangles[i].y + usedRectangles[i].height == y)
					score += CommonIntervalLength(
						(int)usedRectangles[i].x, (int)usedRectangles[i].x + (int)usedRectangles[i].width, x, x + width
					);
			}

			return score;
		}

		Rect FindPositionForNewNodeContactPoint(int width, int height, ref int bestContactScore) {
			var bestNode = new Rect();
			bestContactScore = -1;

			for (var i = 0; i < freeRectangles.Count; ++i) {
				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRectangles[i].width >= width && freeRectangles[i].height >= height) {
					var score = ContactPointScoreNode(
						(int)freeRectangles[i].x, (int)freeRectangles[i].y, width, height
					);
					if (score > bestContactScore) {
						bestNode.x = (int)freeRectangles[i].x;
						bestNode.y = (int)freeRectangles[i].y;
						bestNode.width = width;
						bestNode.height = height;
						bestContactScore = score;
					}
				}

				if (allowRotations && freeRectangles[i].width >= height && freeRectangles[i].height >= width) {
					var score = ContactPointScoreNode(
						(int)freeRectangles[i].x, (int)freeRectangles[i].y, height, width
					);
					if (score > bestContactScore) {
						bestNode.x = (int)freeRectangles[i].x;
						bestNode.y = (int)freeRectangles[i].y;
						bestNode.width = height;
						bestNode.height = width;
						bestContactScore = score;
					}
				}
			}

			return bestNode;
		}

		bool SplitFreeNode(Rect freeNode, ref Rect usedNode) {
			// Test with SAT if the rectangles even intersect.
			if (usedNode.x >= freeNode.x + freeNode.width || usedNode.x + usedNode.width <= freeNode.x ||
			    usedNode.y >= freeNode.y + freeNode.height || usedNode.y + usedNode.height <= freeNode.y)
				return false;

			if (usedNode.x < freeNode.x + freeNode.width && usedNode.x + usedNode.width > freeNode.x) {
				// New node at the top side of the used node.
				if (usedNode.y > freeNode.y && usedNode.y < freeNode.y + freeNode.height) {
					var newNode = freeNode;
					newNode.height = usedNode.y - newNode.y;
					freeRectangles.Add(newNode);
				}

				// New node at the bottom side of the used node.
				if (usedNode.y + usedNode.height < freeNode.y + freeNode.height) {
					var newNode = freeNode;
					newNode.y = usedNode.y + usedNode.height;
					newNode.height = freeNode.y + freeNode.height - (usedNode.y + usedNode.height);
					freeRectangles.Add(newNode);
				}
			}

			if (usedNode.y < freeNode.y + freeNode.height && usedNode.y + usedNode.height > freeNode.y) {
				// New node at the left side of the used node.
				if (usedNode.x > freeNode.x && usedNode.x < freeNode.x + freeNode.width) {
					var newNode = freeNode;
					newNode.width = usedNode.x - newNode.x;
					freeRectangles.Add(newNode);
				}

				// New node at the right side of the used node.
				if (usedNode.x + usedNode.width < freeNode.x + freeNode.width) {
					var newNode = freeNode;
					newNode.x = usedNode.x + usedNode.width;
					newNode.width = freeNode.x + freeNode.width - (usedNode.x + usedNode.width);
					freeRectangles.Add(newNode);
				}
			}

			return true;
		}

		void PruneFreeList() {
			for (var i = 0; i < freeRectangles.Count; ++i)
			for (var j = i + 1; j < freeRectangles.Count; ++j) {
				if (IsContainedIn(freeRectangles[i], freeRectangles[j])) {
					freeRectangles.RemoveAt(i);
					--i;
					break;
				}

				if (IsContainedIn(freeRectangles[j], freeRectangles[i])) {
					freeRectangles.RemoveAt(j);
					--j;
				}
			}
		}

		bool IsContainedIn(Rect a, Rect b) {
			return a.x >= b.x && a.y >= b.y
			                  && a.x + a.width <= b.x + b.width
			                  && a.y + a.height <= b.y + b.height;
		}
	}
}