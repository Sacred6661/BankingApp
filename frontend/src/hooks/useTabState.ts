import { useCallback, useEffect, useState } from "react";

type HistoryMode = "hash" | "query";

interface UseTabStateOptions<T extends string> {
  key?: string; // for query params
  mode?: HistoryMode; // 'hash' | 'query'
  replace?: boolean; // update via replaceState vs assigning location (push)
  defaultTab: T;
  allowedTabs: T[]; // valid list
  onChange?: (tab: T) => void;
}

export function useTabState<T extends string>(options: UseTabStateOptions<T>) {
  const {
    key = "tab",
    mode = "hash",
    replace = false,
    defaultTab,
    allowedTabs,
    onChange,
  } = options;

  const readFromLocation = useCallback((): T => {
    try {
      if (mode === "hash") {
        const raw = (window.location.hash || "").replace("#", "");
        return allowedTabs.includes(raw as T) ? (raw as T) : defaultTab;
      } else {
        const params = new URLSearchParams(window.location.search);
        const raw = params.get(key) || "";
        return allowedTabs.includes(raw as T) ? (raw as T) : defaultTab;
      }
    } catch {
      return defaultTab;
    }
  }, [mode, key, allowedTabs, defaultTab]);

  const [tab, setTab] = useState<T>(() => readFromLocation());

  //sync with url change
  useEffect(() => {
    const onUrlChange = () => {
      const v = readFromLocation();
      setTab(v);
      onChange?.(v);
    };

    if (mode === "hash") {
      window.addEventListener("hashchange", onUrlChange);

      onUrlChange();
      return () => window.removeEventListener("hashchange", onUrlChange);
    } else {

      window.addEventListener("popstate", onUrlChange);
      onUrlChange();
      return () => window.removeEventListener("popstate", onUrlChange);
    }
  }, [mode, readFromLocation, onChange]);

  const setTabAndUrl = useCallback(
    (next: T) => {
      if (!allowedTabs.includes(next)) {
        // ignore invalid
        return;
      }

      if (mode === "hash") {
        if (replace) {
          // replace hash without adding history entry
          const url =
            window.location.pathname + window.location.search + `#${next}`;
          history.replaceState(null, "", url);
        } else {
          window.location.hash = next;
        }
      } else {
        const params = new URLSearchParams(window.location.search);
        if (next === defaultTab) {
          params.delete(key); // clean default
        } else {
          params.set(key, next);
        }
        const newSearch = params.toString();
        const url =
          window.location.pathname + (newSearch ? `?${newSearch}` : "");
        if (replace) history.replaceState(null, "", url);
        else history.pushState(null, "", url);
        // dispatch popstate-like update for other listeners
        window.dispatchEvent(new PopStateEvent("popstate"));
      }

      setTab(next);
      onChange?.(next);
    },
    [allowedTabs, defaultTab, key, mode, replace, onChange]
  );

  return { tab, setTab: setTabAndUrl };
}
