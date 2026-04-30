import { useEffect, useRef, useState } from "react";
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
  const { isAuthenticated } = useSelector((state: RootState) => state.auth);

  const [initialized, setInitialized] = useState(false);
  const fetchingRef = useRef(false);

  useEffect(() => {
    // Reset the redirect status after a successful login
    if (fetchingRef.current) return;

    fetchingRef.current = true;

    const init = async () => {
      try {
        console.log("start provider fetch");
        await dispatch(fetchMe());
        console.log("end provider fetch");
      } catch (err) {
        console.error("fetchMe error:", err);
      } finally {
        setInitialized(true);
      }
    };

    init();

    // Cleanup for StrictMode
    return () => {
      // We don't discard fetchingRef because it's a global variable
    };
  }, [dispatch]);

  // SignalR lifecycle
  useEffect(() => {
    if (!isAuthenticated) {
      stopConnection();
      return;
    }

    startConnection().catch((err) => {
      console.error("SignalR connection failed:", err);
    });

    return () => {
      stopConnection();
    };
  }, [isAuthenticated]);

  if (!initialized) {
    return <LoadingOverlay loading={true} />;
  }

  return <>{children}</>;
}
