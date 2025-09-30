using UnityEngine;

public class MemSubject : MonoBehaviour
{
   [SerializeField] float subjectHealth = 100f;
    public enum MemSubjectType{
        None,
        Tralalelo,
        BalerinaCapuchino,
        Mateo,
    
    };

    public MemSubjectType memSubjectType = MemSubjectType.None;
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    // Применить урон к объекту
    public void ApplyDamage(float damage)
    {
        subjectHealth = Mathf.Max(0f, subjectHealth - Mathf.Max(0f, damage));
        if (subjectHealth <= 0f)
        {
            gameObject.SetActive(false);
        }
    }

    public float GetHealth()
    {
        return subjectHealth;
    }
}
