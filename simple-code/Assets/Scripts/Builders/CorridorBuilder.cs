using UnityEngine;

namespace TestGame.Builders
{
    public sealed class CorridorBuilder : MonoBehaviour
    {
        [Header("Dimensions (meters)")]
        [SerializeField] private float width = 4f;
        [SerializeField] private float length = 20f;
        [SerializeField] private float height = 10f;

        [Header("Materials")]
        [SerializeField] private Material floorMaterial;
        [SerializeField] private Material wallMaterial;

        [Header("Build")]
        [SerializeField] private bool buildOnStart = true;

        public float Width => width;
        public float Length => length;
        public float Height => height;

        public Vector3 EntrancePositionWorld => transform.position + new Vector3(0f, 0f, -length * 0.5f);
        public Vector3 ExitPositionWorld => transform.position + new Vector3(0f, 0f, length * 0.5f);

        private void Start()
        {
            if (buildOnStart) Build();
        }

        public void Build()
        {
            ClearChildren();

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(transform, false);
            floor.transform.localScale = new Vector3(width, 0.2f, length);
            floor.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            ApplyMaterial(floor, floorMaterial);

            CreateWall("Wall Left", new Vector3(-width * 0.5f, height * 0.5f, 0f), new Vector3(0.2f, height, length));
            CreateWall("Wall Right", new Vector3(width * 0.5f, height * 0.5f, 0f), new Vector3(0.2f, height, length));
            CreateWall("Wall Front", new Vector3(0f, height * 0.5f, length * 0.5f), new Vector3(width, height, 0.2f));
            // Back is intentionally open so player can spawn there if desired.
        }

        private void CreateWall(string name, Vector3 localPos, Vector3 localScale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(transform, false);
            wall.transform.localPosition = localPos;
            wall.transform.localScale = localScale;
            ApplyMaterial(wall, wallMaterial);
        }

        private static void ApplyMaterial(GameObject go, Material mat)
        {
            if (mat == null) return;
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
