import * as signalR from "@microsoft/signalr";

export const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7023/hubs/transaction", {
    withCredentials: true,
  })
  .withAutomaticReconnect()
  .build();

let started = false;

export const startConnection = async () => {
  console.log("startConntection");
  if (started) return;

  if (!started) {
    await connection.start();
    started = true;
    console.log("SignalR connected");
  }
};

export const stopConnection = async () => {
  await connection.stop();
};
