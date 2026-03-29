import * as signalR from "@microsoft/signalr";

export const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7023/hubs/transaction", {
    withCredentials: true,
  })
  .withAutomaticReconnect()
  .build();

let started = false;

export const startConnection = async () => {
  try {
    console.log("startConntection");
    if (started) return;

    if (!started) {
      await connection.start();
      started = true;
      console.log("SignalR connected");
    }
  } catch (error) {
    console.log("SignalR error", error);

    if (
      error instanceof Error &&
      error.message.includes("401") &&
      window.location.pathname !== "/login"
    ) {
      window.location.href = "/login";
    }
  }
};

export const stopConnection = async () => {
  await connection.stop();
};
