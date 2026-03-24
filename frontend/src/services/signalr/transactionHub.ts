import { connection } from "./connection";

export const subscribeToTransaction = async (transactionId: string) => {
  await connection.invoke("SubscribeToTransaction", transactionId);
};

export const onTransactionUpdated = (callback: (data: any) => void) => {
  connection.on("TransactionUpdated", callback);
};

export const offTransactionUpdated = (callback: any) => {
  connection.off("TransactionUpdated", callback);
};
