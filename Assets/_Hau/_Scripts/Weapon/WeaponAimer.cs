using UnityEngine;

public class WeaponAimer : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform playerTransform;
    public Transform weaponPivot;  // new
    private Transform currentWeapon;

    //[SerializeField] private bool flipSprite = true;

    [SerializeField] private Vector3 weaponOffset = new Vector3(0.806f, 0f, 0f);

    private int lastFacingDirection = 1; // 1 = phải, -1 = trái

    void Update()
    {
        if (currentWeapon == null || !currentWeapon.gameObject.activeSelf) return;

        // AimAtMouse hiện tại đã bao gồm FlipPlayer → ta sẽ tách riêng ra xử lý theo thứ tự chuẩn hơn
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        FlipPlayerIfNeeded(mouseWorldPos); // ⬅ Gọi trước
        AimAtMouse(mouseWorldPos);         // ⬅ Rồi mới xoay
    }


    void AimAtMouse(Vector3 mouseWorldPos)
    {
        if (weaponHolder == null || Camera.main == null) return;

        Vector3 direction = mouseWorldPos - weaponHolder.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        int currentDirection = (int)Mathf.Sign(playerTransform.localScale.x);

        if (currentDirection < 0)
        {
            angle = 180 - angle;
        }

        weaponHolder.rotation = Quaternion.Euler(0, 0, angle);

        // ✅ Đảm bảo update vị trí vũ khí mỗi frame
        UpdateWeaponOffset(currentDirection);
    }




    void FlipPlayerIfNeeded(Vector3 mousePos)
    {
        if (playerTransform == null) return;

        float mouseDirection = mousePos.x - playerTransform.position.x;
        float currentFacing = playerTransform.localScale.x;

        // Nếu hướng chuột khác với hướng nhìn hiện tại → flip player
        if ((mouseDirection < 0 && currentFacing > 0) || (mouseDirection > 0 && currentFacing < 0))
        {
            Vector3 scale = playerTransform.localScale;
            scale.x *= -1;
            playerTransform.localScale = scale;

            // Cập nhật lại vị trí weapon theo hướng mới
            UpdateWeaponOffset((int)Mathf.Sign(scale.x));
        }
    }

    void UpdateWeaponOffset(int direction)
    {
        if (weaponPivot == null) return;

        weaponPivot.localPosition = new Vector3(
            weaponOffset.x * direction,
            weaponOffset.y,
            weaponOffset.z
        );

        lastFacingDirection = direction;
    }


    public void SetCurrentWeapon(Transform weapon)
    {
        currentWeapon = weapon;
        UpdateWeaponOffset((int)Mathf.Sign(playerTransform.localScale.x));
    }

}


