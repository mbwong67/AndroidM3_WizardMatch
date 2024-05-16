using UnityEngine;

namespace WizardMatch
{
    public class GolemSmash : AttackHitbox
    {

        void Awake()
        {
            
        }
        void Update()
        {
            transform.position = targetCharacter.transform.position;
            CheckBoxBounds();
        }
        void CheckBoxBounds()
        {
            var col = Physics2D.OverlapBox(transform.position,_boxCol.size,0,_filter);
            if (col && col.GetComponent<Character>() == targetCharacter)
            {
                SwapRandomTokens();
                DealDamage();
                KillHitBox();
            }
        }
        void KillHitBox()
        {
            Destroy(gameObject);
        }

        void SwapRandomTokens()
        {
            var board = FindObjectOfType<GameBoard>();
            var tokens = board.playFieldTokens;

            bool flag = true;
            while (flag)
            {
                var randomX_1 = Random.Range(0,tokens.GetLength(0) - 1);
                var randomY_1 = Random.Range(0,tokens.GetLength(1) - 1);

                int randomX_2, randomY_2;
                while (true)
                {
                    randomX_2 = Random.Range(0,tokens.GetLength(0) - 1);
                    randomY_2 = Random.Range(0,tokens.GetLength(1) - 1);

                    if (randomX_2 != randomX_1 && randomY_2 != randomY_1)
                        break;
                }
                var token_1 = tokens[randomX_1,randomY_1];
                var token_2 = tokens[randomX_2,randomY_2];

                if (
                    token_1.CountNeighborsInCertainDirection(token_1,SwipeDirection.LEFT) + token_1.CountNeighborsInCertainDirection(token_1,SwipeDirection.RIGHT) < 2
                &&  token_1.CountNeighborsInCertainDirection(token_1,SwipeDirection.UP)   + token_1.CountNeighborsInCertainDirection(token_1,SwipeDirection.DOWN)  < 2 
                &&  token_2.CountNeighborsInCertainDirection(token_2,SwipeDirection.LEFT) + token_2.CountNeighborsInCertainDirection(token_2,SwipeDirection.RIGHT) < 2
                &&  token_2.CountNeighborsInCertainDirection(token_1,SwipeDirection.UP)   + token_2.CountNeighborsInCertainDirection(token_2,SwipeDirection.DOWN)  < 2)
                {
                    token_1.SwapTokenPositions(token_1,token_2);
                    flag = false;
                }
            }   
        }
    }
}