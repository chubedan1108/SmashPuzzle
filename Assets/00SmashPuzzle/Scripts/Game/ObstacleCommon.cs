using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCommon : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject m_mesh;
    [SerializeField] private GameObject m_renderer;
    [SerializeField] private GameObject m_brokenPiecesRoot;
    [SerializeField] private List<GameObject> m_brokenPieces = new List<GameObject>();
    [SerializeField] private float effectDuration = 2f;
    [Header("Break on Ground")]
    [SerializeField] private bool m_breakOnGround = true;

    [Header("Break Physics Parameters")]
    public float BreakForce = 8f;        // Luc no hat vang manh vo
    public float BreakRadius = 2f;       // Ban kinh no manh vo
    public float BreakUpwards = 0.1f;    // Luc hat bong manh vo len troi
    public float BreakMaxSpeed = 3f;     // Toc do toi da cua manh vo
    public float BreakSpin = 450f;       // Do xoay goc ngau nhien cua manh vo

    public GameObject Mesh => m_mesh;
    public GameObject Renderer => m_renderer;
    public GameObject BrokenPiecesRoot => m_brokenPiecesRoot;
    public List<GameObject> BrokenPieces => m_brokenPieces;

    private bool m_isBroken;

    private void Awake()
    {
        m_brokenPiecesRoot.SetActive(false);
        m_mesh.SetActive(true);
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.GetComponent<Ground>())
    //    {
    //        Vector3 contactPoint = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
    //        Break(contactPoint);
    //    }

    //}

    // Kich hoat vo vun
    public void Break(Vector3 explosionCenter, float strengthMultiplier = 1f)
    {
        if (m_isBroken) return;
        m_isBroken = true;
        SpawnBrokenPieces(explosionCenter, strengthMultiplier);
        EffectAfterBreak();
    }

    private void EffectAfterBreak()
    {
        m_renderer.transform.DOScale(0f, 2f)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }
    
    //Kich hoat hien thi cach manh vo (nam yen, no tanh banh)
    public void SpawnBrokenPieces(Vector3 explosionCenter, float strengthMultiplier = 1f)
    {
        Debug.Log("Spawn broken pieces");
        m_mesh.SetActive(false);
        m_brokenPiecesRoot.SetActive(true);
   
        //m_isBroken = true;
        //Debug.Log($"[ObstacleCommon] SpawnBrokenPieces() hiển thị visual mảnh vỡ trên '{gameObject.name}'!");

        //// 1. Hiển thị cây mảnh vỡ (giữ nguyên làm con của GameObject)
        //if (m_brokenPiecesRoot != null)
        //{
        //    m_brokenPiecesRoot.SetActive(true);
        //}
        //else
        //{
        //    Debug.LogWarning($"[ObstacleCommon] m_brokenPiecesRoot đang null trên '{gameObject.name}'!");
        //}

        //// 3. Bật tất cả các mảnh vỡ con và đảm bảo tắt chuyển động Rigidbody trên từng mảnh (nếu có)
        //if ((m_brokenPieces == null || m_brokenPieces.Count == 0) && m_brokenPiecesRoot != null)
        //{
        //    m_brokenPieces = new List<GameObject>();
        //    foreach (Transform child in m_brokenPiecesRoot.transform)
        //    {
        //        m_brokenPieces.Add(child.gameObject);
        //    }
        //}

        //if (m_brokenPieces != null)
        //{
        //    foreach (var piece in m_brokenPieces)
        //    {
        //        if (piece != null)
        //        {
        //            piece.SetActive(true);
        //            if (piece.TryGetComponent<Rigidbody>(out var pieceRb))
        //            {
        //                pieceRb.linearVelocity = Vector3.zero;
        //                pieceRb.angularVelocity = Vector3.zero;
        //                pieceRb.isKinematic = true;
        //            }
        //        }
        //    }
        //}
    }
}
