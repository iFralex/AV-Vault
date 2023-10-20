using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SocialApp
{
    public class LoadingController : MonoBehaviour
    {

        [SerializeField]
        private Transform RotatorTarget = default;
        [SerializeField]
        private float Speed = default;

        void Update()
        {
            //RotatorTarget.Rotate(new Vector3(0, 0, Speed * Time.deltaTime));
        }
    }
}