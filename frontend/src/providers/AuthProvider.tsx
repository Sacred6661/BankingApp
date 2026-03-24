import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { fetchMe } from "../features/auth/authSlice";
import type { AppDispatch, RootState } from "../app/store";
import LoadingOverlay from "../components/ui/LoadingOverlay";

import {
  startConnection,
  stopConnection,
} from "../services/signalr/connection";

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const dispatch = useDispatch<AppDispatch>();
  const { loading, isAuthenticated } = useSelector(
    (state: RootState) => state.auth,
  );

  const [initialized, setInitialized] = useState(false);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        await dispatch(fetchMe());
      } catch (err) {
        // 401 and other errors ignore
      } finally {
        if (!cancelled) setInitialized(true); // to set only once
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [dispatch]);

  // 🔥 SignalR lifecycle
  useEffect(() => {
    console.log("SignalR useEffect");
    if (!isAuthenticated) return;

    startConnection().catch((err) => {
      console.error("SignalR connection failed:", err);
    });

    return () => {
      stopConnection();
    };
  }, [isAuthenticated]);

  if (!initialized) {
    return (
      <>
        <LoadingOverlay loading={!initialized || loading} />
        {initialized && children}
      </>
    );
  }

  return <>{children}</>;
}
