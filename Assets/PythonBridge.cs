using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Concurrent;

public class PythonBridge : MonoBehaviour
{
    public Transform player;
    public string obstacleTag = "Obstacle";

    private TcpListener stateListener;
    private TcpListener actionListener;
    private Thread stateThread;
    private Thread actionThread;

    private readonly ConcurrentQueue<TcpClient> stateClients = new ConcurrentQueue<TcpClient>();
    private readonly ConcurrentQueue<string> actionQueue = new ConcurrentQueue<string>();

    private Vector3 startPosition;
    private float resetCooldown = 0f;
    private bool running = true;

    void Start()
    {
        startPosition = player.position;

        stateListener = new TcpListener(IPAddress.Any, 5555);
        actionListener = new TcpListener(IPAddress.Any, 5556);

        stateListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        actionListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        stateListener.Start();
        actionListener.Start();

        running = true;
        stateThread = new Thread(ListenForStateRequests);
        actionThread = new Thread(ListenForActions);
        stateThread.Start();
        actionThread.Start();

        Debug.Log("🟢 PythonBridge started.");
    }

    void OnApplicationQuit()
    {
        Shutdown();
    }

    void OnDisable()
    {
        Shutdown();
    }

    void Shutdown()
    {
        Debug.Log("🛑 Shutting down PythonBridge...");

        running = false;

        try { stateListener?.Stop(); } catch {}
        try { actionListener?.Stop(); } catch {}

        try { stateThread?.Interrupt(); stateThread?.Join(100); } catch {}
        try { actionThread?.Interrupt(); actionThread?.Join(100); } catch {}

        Debug.Log("🧹 Cleanup complete.");
    }

    void ListenForStateRequests()
    {
        while (running)
        {
            try
            {
                TcpClient client = stateListener.AcceptTcpClient();
                stateClients.Enqueue(client);
            }
            catch (SocketException)
            {
                if (!running) break;
                Debug.LogWarning("⚠️ State socket closed.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("❌ StateThread error: " + ex.Message);
            }
        }
    }

    void ListenForActions()
    {
        while (running)
        {
            TcpClient client = null;
            try
            {
                client = actionListener.AcceptTcpClient();
                Debug.Log("✅ Action client connected.");
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[128];

                while (client.Connected && running)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string action = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                        actionQueue.Enqueue(action);
                        Debug.Log("🎮 Received action: " + action);
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            catch (SocketException)
            {
                if (!running) break;
                Debug.LogWarning("⚠️ Action socket closed.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("❌ ActionThread error: " + ex.Message);
            }
            finally
            {
                client?.Close();
                Debug.Log("❌ Action client disconnected.");
            }
        }
    }

void Update()
{
    if (resetCooldown > 0f)
    {
        resetCooldown -= Time.deltaTime;
    }

    // Send state to Python
    while (stateClients.TryDequeue(out TcpClient client))
    {
        try
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                float px = player.position.x;
                float py = player.position.y;

                // Nearest obstacle
                float closestObstacle = float.MaxValue;
                foreach (var obj in GameObject.FindGameObjectsWithTag("Obstacle"))
                {
                    float d = Vector3.Distance(player.position, obj.transform.position);
                    if (d < closestObstacle)
                        closestObstacle = d;
                }

                // Nearest coin or diamond
                float closestCoin = float.MaxValue;
                foreach (var obj in GameObject.FindGameObjectsWithTag("coin"))
                {
                    float d = Vector3.Distance(player.position, obj.transform.position);
                    if (d < closestCoin)
                        closestCoin = d;
                }

                foreach (var obj in GameObject.FindGameObjectsWithTag("diamond"))
                {
                    float d = Vector3.Distance(player.position, obj.transform.position);
                    if (d < closestCoin)
                        closestCoin = d;
                }

                // Done and reward
                var movement = player.GetComponent<playermovement>();
                bool done = (movement != null) ? !movement.runn : true;

                float reward = 0.01f;
                if (done) reward = -1f;

                string msg = $"{px},{py},{closestObstacle},{closestCoin}|{reward}|{done}\n";
                byte[] data = Encoding.ASCII.GetBytes(msg);
                stream.Write(data, 0, data.Length);

                Debug.Log("📤 Sent state to Python: " + msg.Trim());
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("❌ Error sending state: " + ex.Message);
        }
    }

    // Handle actions
    while (actionQueue.TryDequeue(out string action))
    {
        if (int.TryParse(action, out int a))
        {
            if (resetCooldown > 0f)
            {
                Debug.LogWarning($"⏳ Ignoring action {a} during cooldown.");
                continue;
            }

            var movement = player.GetComponent<playermovement>();

            if (!player.gameObject.activeSelf && a == 0)
            {
                ResetPlayer();
            }
            else if (movement != null)
            {
                if (a == 1) movement.MoveLeft();
                else if (a == 2) movement.MoveRight();
                else if (a == 3) movement.Jump();
            }
        }
    }
}

    void ResetPlayer()
    {
        Debug.Log("🔄 ResetPlayer()");

        player.position = startPosition;
        player.rotation = Quaternion.identity;
        player.gameObject.SetActive(true);

        resetCooldown = 1.0f;
        StartCoroutine(EnableMovementNextFrame());
    }

    IEnumerator EnableMovementNextFrame()
    {
        yield return new WaitForSeconds(0.5f);
        var movement = player.GetComponent<playermovement>();
        if (movement != null)
        {
            movement.runn = true;
            movement.anim.SetBool("run", true);
            movement.check = true;
            Debug.Log("🏃 Player resumed.");
        }
    }
}
