import { Box, Button, Card, Grid, Typography } from "@mui/material";
import ReadOnlyCopyField from "../../../components/ui/ReadOnlyCopyField";
import type { ProfileContactDto } from "../../../api/profileService";

interface Props {
  contacts: ProfileContactDto[];
  onEdit: (contact: ProfileContactDto) => void;
  onDelete: (id: number) => void;
  onAdd: () => void;
}

export default function ContactsTab({
  contacts,
  onEdit,
  onDelete,
  onAdd,
}: Props) {
  console.log(contacts);

  return (
    <Box p={2}>
      <Typography variant="h6" mb={2}>
        Contacts
      </Typography>
      <Button variant="outlined" size="small" onClick={onAdd}>
        + Add Contact
      </Button>
      <Grid spacing={2} mt={1}>
        {contacts
          .filter((c: ProfileContactDto) => c.isActive)
          .sort((a, b) => a.contactTypeId - b.contactTypeId)
          .map((contact: ProfileContactDto, index: number) => (
            <Grid key={contact.id || index} size={{ xs: 6, md: 4 }}>
              <Card variant="outlined" sx={{ p: 2 }}>
                <Typography variant="subtitle2">
                  {contact.contactTypeName}
                </Typography>
                <ReadOnlyCopyField value={contact.value} />

                <Box mt={1}>
                  {contact.contactTypeId !== 1 && (
                    <>
                      <Button size="small" onClick={() => onEdit(contact)}>
                        Edit
                      </Button>
                      <Button
                        size="small"
                        color="error"
                        onClick={() => onDelete(contact.id!)}
                      >
                        Delete
                      </Button>
                    </>
                  )}
                </Box>
              </Card>
            </Grid>
          ))}
      </Grid>
    </Box>
  );
}
