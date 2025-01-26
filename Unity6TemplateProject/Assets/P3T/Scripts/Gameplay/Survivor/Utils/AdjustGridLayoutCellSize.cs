using UnityEngine;
using UnityEngine.UI;

namespace P3T.Scripts.Gameplay.Survivor
{
	[RequireComponent(typeof(GridLayoutGroup))]
	public class AdjustGridLayoutCellSize : AdjustGridCellSize
	{
		private GridLayoutGroup _grid;

		protected override bool HasGridReference => _grid != null;
		protected override int GridConstraintCount => _grid.constraintCount;
		protected override Vector2 GridSpacing
		{
			get => _grid.spacing;
			set => _grid.spacing = value;
		}
		protected override Vector2 GridCellSize
		{
			get => _grid.cellSize;
			set => _grid.cellSize = value;
		}
		protected override RectOffset GridPadding
		{
			get => _grid.padding;
			set => _grid.padding = value;
		}

		protected override void GetGridReference()
		{
			if(_grid == null) _grid = GetComponent<GridLayoutGroup>();
		}
	}
}