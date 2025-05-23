using UnityEngine;

public class MapBuilder : MonoBehaviour
{
    [System.Serializable]
    public class BlockData
    {
        public Vector2 position;
        public BlockLegData legData;
        public GameObject prefab;
    }

    [System.Serializable]
    public class LevelData
    {
        public BlockData[] blocks;
    }

    public LevelData[] levels; // Gắn dữ liệu từng màn
    public Transform[] levelParents; // Gắn với panel trong MinigameManager

    void Start()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            var parent = levelParents[i];
            foreach (var block in levels[i].blocks)
            {
                GameObject obj = Instantiate(block.prefab, parent);
                obj.transform.localPosition = block.position;

                Block blockComp = obj.GetComponent<Block>();
                blockComp.legData = CloneLegData(block.legData);

                // Nếu cần gắn loại khối (fixed, rotate, v.v.) thì thêm ở đây
                BlockController ctrl = obj.GetComponent<BlockController>();
                if (ctrl != null)
                    ctrl.blockType = BlockController.BlockType.MoveRotate; // hoặc Fixed, MoveOnly, v.v.
            }
        }
    }

    private BlockLegData CloneLegData(BlockLegData source)
    {
        return new BlockLegData
        {
            up = source.up,
            right = source.right,
            down = source.down,
            left = source.left
        };
    }
}
