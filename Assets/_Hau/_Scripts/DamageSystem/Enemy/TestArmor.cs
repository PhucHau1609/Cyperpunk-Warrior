using UnityEngine;

public class TestArmor : DamageReceiver
{
    [SerializeField] private int armor = 1; // giáp mặc định

    public override int Deduct(int damage)
    {
        // Nếu damage nhỏ hơn hoặc bằng giáp, không trừ máu
        if (damage <= armor)
        {
            //Debug.Log("Damage blocked by armor!");
            return currentHP;
        }

        // Trừ damage sau khi đã vượt qua giáp
        return base.Deduct(damage);
    }
}
