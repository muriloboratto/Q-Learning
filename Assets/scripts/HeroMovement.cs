using UnityEngine;

public class HeroMovement : MonoBehaviour
{
    public Transform hero;
   
    public bool moveHero = false; // Q-Learning tabular: alvo fixo por padrao
   
    void Update()
    {
        if (!moveHero || hero == null) 
           return;

    }
}