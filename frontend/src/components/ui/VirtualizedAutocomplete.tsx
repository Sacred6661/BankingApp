import * as React from "react";
import { TextField, Autocomplete } from "@mui/material";
import { useVirtualizer } from "@tanstack/react-virtual";

interface VirtualizedAutocompleteProps<T> {
  value: T | null;
  options: T[];
  getOptionLabel: (option: T) => string;
  onChange: (value: T | null) => void;
  label: string;
  error?: boolean;
  helperText?: string;
  isOptionEqualToValue?: (option: T, value: T) => boolean;
  height?: number;
  itemHeight?: number;
}

// need to use forwardRef for next steps
const VirtualizedListboxComponent = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLElement>
>(function VirtualizedListboxComponent(props, _ref) {
  const { children, ...other } = props;
  const parentRef = React.useRef<HTMLDivElement>(null);

  // make <li> massive
  const items = React.Children.toArray(children);

  const rowVirtualizer = useVirtualizer({
    count: items.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => 36,
    overscan: 5,
  });

  return (
    <div
      ref={parentRef}
      {...other}
      style={{
        position: "relative",
        overflow: "auto",
        maxHeight: 200,
      }}
    >
      <div
        style={{
          height: rowVirtualizer.getTotalSize(),
          position: "relative",
          width: "100%",
        }}
      >
        {rowVirtualizer.getVirtualItems().map((virtualRow) => (
          <div
            key={virtualRow.key}
            style={{
              position: "absolute",
              top: 0,
              left: 0,
              width: "100%",
              transform: `translateY(${virtualRow.start}px)`,
            }}
          >
            {items[virtualRow.index]}
          </div>
        ))}
      </div>
    </div>
  );
});

export function VirtualizedAutocomplete<T>({
  value,
  options,
  getOptionLabel,
  onChange,
  label,
  error,
  helperText,
  isOptionEqualToValue,
}: VirtualizedAutocompleteProps<T>) {
  return (
    <Autocomplete
      value={value}
      options={options}
      getOptionLabel={getOptionLabel}
      isOptionEqualToValue={isOptionEqualToValue}
      onChange={(_, newValue) => onChange(newValue)}
      renderInput={(params) => (
        <TextField {...params} label={label} error={error} helperText={helperText} />
      )}
      ListboxComponent={VirtualizedListboxComponent}
    />
  );
}
