using UnityEngine;

namespace Demo
{
    public class BillboardStock : Shape
    {
        // grammar rule probabilities:
        public float stockContinueChance = 0.0f;
        public int MaxHeight = 3;

        // shape parameters:
        public int Width;
        public int Depth;
        public int PlacementSide;

        [SerializeField] LodObject[] edgeStyle;
        [SerializeField] LodObject[] bodyStyle;

        int _currentHeight;

        public void Initialize(int placementSide ,int Width, int Depth, int maxHeight)
        {
            this.Width = Width;
            this.Depth = Depth;
            this.PlacementSide = placementSide;
            MaxHeight = maxHeight;

            if(PlacementSide % 2 == 1)
                this.Width = RandomInt(1,Width);
            else
                this.Depth = RandomInt(1,Depth);
        }
        void Initialize(int placementSide,
            int Width,
            int Depth,
            LodObject[] bodyStyle,
            LodObject[] edgeStyle,
            int maxHeight,
            int currentHeight)
        {
            this.Width = Width;
            this.Depth = Depth;
            this.edgeStyle = edgeStyle;
            this.bodyStyle = bodyStyle;
            this.PlacementSide = placementSide;
            MaxHeight = maxHeight;
            _currentHeight = currentHeight;
        }

        protected override void Execute()
        {
            // Create four walls:
            Vector3 localPosition = new();
            switch (PlacementSide)
            {
                case 0:
                    localPosition = new Vector3(-(Width - 1) * 0.5f, 0, 0); // left
                    break;
                case 1:
                    localPosition = new Vector3(0, 0, (Depth - 1) * 0.5f); // back
                    break;
                case 2:
                    localPosition = new Vector3((Width - 1) * 0.5f, 0, 0); // right
                    break;
                case 3:
                    localPosition = new Vector3(0, 0, -(Depth - 1) * 0.5f); // front
                    break;
            }
            if (_currentHeight == 0 || _currentHeight == MaxHeight)
            {
                SimpleRow edgeRow = CreateSymbol<SimpleRow>("wall", localPosition, Quaternion.Euler(0, PlacementSide * 90, 0));
                edgeRow.Initialize(PlacementSide % 2 == 1 ? Width : Depth, edgeStyle);
                edgeRow.Generate();
            }
            else
            {
                SimpleRow newRow = CreateSymbol<SimpleRow>("wall", localPosition, Quaternion.Euler(0, PlacementSide * 90, 0));
                newRow.Initialize(PlacementSide % 2 == 1 ? Width : Depth, bodyStyle);
                newRow.Generate();
            }

            if (_currentHeight == MaxHeight - 1) return;

            float randomValue = RandomFloat();
            if (randomValue < stockContinueChance)
            {
                BillboardStock nextStock = CreateSymbol<BillboardStock>("stock", new Vector3(0, -1, 0));
                nextStock.stockContinueChance = stockContinueChance;
                nextStock.Initialize
                (
                    PlacementSide, Width, Depth, 
                    bodyStyle, edgeStyle, MaxHeight,
                    _currentHeight + 1
                );
                nextStock.Generate(buildDelay);
            }
        }
    }
}
