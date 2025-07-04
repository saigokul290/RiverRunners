import socket
import time
import random

def create_state_socket():
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect(('localhost', 5555))
    return s

def create_action_socket():
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect(('localhost', 5556))
    print("✅ Connected to action socket")
    return s

def receive_state(sock):
    try:
        data = sock.recv(1024).decode().strip()
        print(f"📦 Raw data received: {data}")

        parts = data.split("|")
        if len(parts) != 3:
            print("❌ Unexpected format (expected 3 parts):", parts)
            return None, 0.0, True

        state_values = list(map(float, parts[0].split(",")))
        reward = float(parts[1])
        done = parts[2].strip().lower() == "true"

        return state_values, reward, done

    except Exception as e:
        print("❌ Error in receive_state:", e)
        return None, 0.0, True

def send_action(sock, action):
    try:
        sock.sendall(f"{action}\n".encode())
    except Exception as e:
        print(f"⚠️ Failed to send action: {e}. Reconnecting...")
        try:
            sock.close()
        except:
            pass
        time.sleep(0.5)
        return create_action_socket()
    return sock

# ----- Main Loop -----

actions = [0, 1, 2, 3]
episode = 0
reward_sum = 0

action_socket = create_action_socket()

while True:
    try:
        state_socket = create_state_socket()
        state, reward, done = receive_state(state_socket)
        state_socket.close()

        if state is None:
            raise Exception("Invalid state received")

        print(f"🧠 State: {state} | 💰 Reward: {reward} | ✅ Done: {done}")
        reward_sum += reward

        if done:
            episode += 1
            print(f"\n🔁 Episode {episode} finished with total reward: {round(reward_sum, 2)}")
            reward_sum = 0
            action = 0
            time.sleep(1.0)
        else:
            action = random.choice(actions)

        action_socket = send_action(action_socket, action)
        print(f"🎮 Sent action: {action}")
        time.sleep(0.1)

    except Exception as e:
        print("⚠️ Main loop error:", e)
        time.sleep(0.5)
